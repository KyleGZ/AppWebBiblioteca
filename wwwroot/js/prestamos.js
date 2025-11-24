// Configuración de URLs (rutas MVC internas)
const PRESTAMOS_CONFIG = {
    urls: {
        prestamos: '/Prestamos/GetPrestamos',
        prestamosActivos: '/Prestamos/GetPrestamosActivos',
        devolucion: '/Prestamos/RegistrarDevolucion', // se usa con ?idPrestamo=
        renovar: '/Prestamos/Renovar',                // se usa con ?idPrestamo=&nuevaFechaVencimiento=
        create: '/Prestamos/Create',
        buscar: '/Prestamos/Buscar'                   // NUEVO: endpoint de búsqueda
    }
};

class PrestamosManager {
    constructor() {
        this.vistaActual = 'tarjetas';
        this.prestamosData = [];
        this.init();
    }

    init() {
        this.setupEventListeners();
        const formRenovar = document.getElementById('formRenovarPrestamo');
        if (formRenovar) formRenovar.addEventListener('submit', (e) => this.renovarPrestamoSubmit(e));
        this.cargarPrestamos();
    }

    setupEventListeners() {
        document.getElementById('btnRegistrarDevolucion')?.addEventListener('click', () => this.toggleSeccionDevoluciones());
        document.getElementById('formNuevoPrestamo')?.addEventListener('submit', (e) => this.handleFormSubmit(e));
        document.getElementById('vista-tarjetas')?.addEventListener('click', () => this.cambiarVista('tarjetas'));
        document.getElementById('vista-tabla')?.addEventListener('click', () => this.cambiarVista('tabla'));
        
        // MODIFICADO: Evento para buscar con Enter
        document.getElementById('searchInput')?.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                this.buscarPrestamos();
            }
        });
        
        // NUEVO: Evento para botón de búsqueda
        document.getElementById('btnBuscar')?.addEventListener('click', () => this.buscarPrestamos());
        
        document.getElementById('filtroEstado')?.addEventListener('change', () => this.filtrarPrestamos());
        document.getElementById('btnLimpiarFiltros')?.addEventListener('click', () => this.limpiarFiltros());
    }

    async cargarPrestamos() {
        try {
            const response = await fetch(PRESTAMOS_CONFIG.urls.prestamos, { headers: { 'Accept': 'application/json' } });
            const raw = await response.text();
            let data; try { data = JSON.parse(raw); } catch { }

            if (!response.ok) {
                const msg = (data && (data.message || data.mensaje || data.error)) || raw || 'Error desconocido';
                this.renderError(msg);
                return;
            }

            // NUEVO: Ordenar por fecha más reciente
            if (data && Array.isArray(data)) {
                data.sort((a, b) => {
                    const fechaA = new Date(a.FechaPrestamo || a.fechaPrestamo);
                    const fechaB = new Date(b.FechaPrestamo || b.fechaPrestamo);
                    return fechaB - fechaA; // Más reciente primero
                });
            }

            this.prestamosData = data;
            this.actualizarEstadisticas(data);
            this.mostrarPrestamos(data);
        } catch (e) {
            this.renderError(e.message);
        }
    }

    // MODIFICADO: Método para buscar préstamos - CORREGIDO el parámetro a 'termino'
    async buscarPrestamos() {
        const searchInput = document.getElementById('searchInput');
        const query = searchInput?.value.trim();

        if (!query) {
            showError('Por favor ingrese un criterio de búsqueda');
            return;
        }

        try {
            // Mostrar indicador de carga
            document.getElementById('lista-prestamos').innerHTML = `
                <div class="text-center py-5">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Buscando...</span>
                    </div>
                    <p class="mt-2 text-muted">Buscando préstamos...</p>
                </div>`;

            // CORREGIDO: Usar 'termino' en lugar de 'q'
            const url = `${PRESTAMOS_CONFIG.urls.buscar}?termino=${encodeURIComponent(query)}`;
            
            const response = await fetch(url, { 
                headers: { 'Accept': 'application/json' } 
            });

            const raw = await response.text();
            let data;
            try { 
                data = JSON.parse(raw); 
            } catch { }

            if (!response.ok) {
                const msg = (data && (data.message || data.mensaje || data.error)) || raw || 'Error al buscar';
                this.renderError(msg);
                return;
            }

            // La respuesta puede venir como { success: true, data: [...] } o directamente como array
            const resultados = data.data || data;

            if (!resultados || resultados.length === 0) {
                document.getElementById('lista-prestamos').innerHTML = `
                    <div class="alert alert-info text-center">
                        <i class="fas fa-search fa-2x mb-2"></i>
                        <h5>No se encontraron resultados</h5>
                        <p class="mb-0">No hay préstamos que coincidan con: <strong>"${query}"</strong></p>
                    </div>`;
                return;
            }

            this.prestamosData = resultados;
            this.actualizarEstadisticas(resultados);
            this.mostrarPrestamos(resultados);
            
            showSuccess(`Se encontraron ${resultados.length} resultado(s)`);

        } catch (err) {
            this.renderError('Error de conexión: ' + err.message);
        }
    }

    // MODIFICADO: Método para limpiar filtros
    limpiarFiltros() {
        // Limpiar campo de búsqueda
        const searchInput = document.getElementById('searchInput');
        if (searchInput) searchInput.value = '';
        
        // Limpiar filtro de estado
        const filtroEstado = document.getElementById('filtroEstado');
        if (filtroEstado) filtroEstado.value = '';
        
        // Recargar todos los préstamos
        this.cargarPrestamos();
        
        showSuccess('Filtros limpiados');
    }

    // MODIFICADO: Método para filtrar préstamos localmente
    filtrarPrestamos() {
        const searchValue = document.getElementById('searchInput')?.value.toLowerCase().trim() || '';
        const estadoValue = document.getElementById('filtroEstado')?.value || '';

        if (!searchValue && !estadoValue) {
            // Si no hay filtros, mostrar todos
            this.mostrarPrestamos(this.prestamosData);
            return;
        }

        let filtrados = this.prestamosData;

        // Filtrar por texto (búsqueda local)
        if (searchValue) {
            filtrados = filtrados.filter(p => {
                const libro = (p.LibroTitulo || p.libroTitulo || '').toLowerCase();
                const usuario = (p.UsuarioNombre || p.usuarioNombre || '').toLowerCase();
                const isbn = (p.LibroIsbn || p.libroIsbn || '').toLowerCase();
                
                return libro.includes(searchValue) || 
                       usuario.includes(searchValue) || 
                       isbn.includes(searchValue);
            });
        }

        // Filtrar por estado
        if (estadoValue) {
            filtrados = filtrados.filter(p => {
                const estado = p.Estado || p.estado || 'Activo';
                return estado === estadoValue;
            });
        }

        this.mostrarPrestamos(filtrados);
    }

    renderError(msg) {
        document.getElementById('lista-prestamos').innerHTML =
          `<div class="alert alert-danger"><strong>Error:</strong> ${msg}</div>`;
    }

    actualizarEstadisticas(prestamos) {
        const hoy = new Date();
        const activos = prestamos.filter(p => (p.Estado || p.estado) === 'Activo');
        const vencidos = activos.filter(p => new Date(p.FechaVencimiento || p.fechaVencimiento) < hoy);
        const devueltosHoy = prestamos.filter(p => {
            const fd = p.FechaDevolucion || p.fechaDevolucion;
            if (!fd) return false;
            return (p.Estado || p.estado) === 'Devuelto' && new Date(fd).toDateString() === hoy.toDateString();
        });

        document.getElementById('total-prestamos').textContent = prestamos.length;
        document.getElementById('prestamos-activos').textContent = activos.length;
        document.getElementById('prestamos-vencidos').textContent = vencidos.length;
        document.getElementById('devoluciones-hoy').textContent = devueltosHoy.length;
    }

    cambiarVista(v) {
        this.vistaActual = v;
        document.getElementById('vista-tarjetas')?.classList.toggle('active', v === 'tarjetas');
        document.getElementById('vista-tabla')?.classList.toggle('active', v === 'tabla');
        this.mostrarPrestamos(this.prestamosData);
    }

    mostrarPrestamos(prestamos) {
        if (this.vistaActual === 'tarjetas')
            this.mostrarPrestamosEnTarjetas(prestamos);
        else
            this.mostrarPrestamosEnTabla(prestamos);
    }

    getPrestamoId(p) {
        return p.Id ?? p.id ?? p.IdPrestamo ?? p.idPrestamo ?? p.PrestamoId ?? p.prestamoId ?? null;
    }

    // Vistas generales (tarjetas y tabla) --------------------------
    mostrarPrestamosEnTarjetas(prestamos) {
        const container = document.getElementById('lista-prestamos');
        if (!container) return;
        if (!prestamos || prestamos.length === 0) {
            container.innerHTML = `
              <div class="empty-state text-center py-5">
                <i class="fas fa-book-open fa-4x text-muted mb-3"></i>
                <h4>No hay préstamos registrados</h4>
                <p class="text-muted">Registra el primer préstamo para comenzar</p>
                <button class="btn btn-esparza-primary mt-3" data-bs-toggle="modal" data-bs-target="#modalNuevoPrestamo">
                  <i class="fas fa-plus me-2"></i>Nuevo Préstamo
                </button>
              </div>`;
            return;
        }
        let html = '<div class="row g-3">';
        prestamos.forEach(p => {
            const libro = p.LibroTitulo || p.libroTitulo || `Libro ${p.LibroId || p.libroId}`;
            const usuario = p.UsuarioNombre || p.usuarioNombre || `Usuario ${p.UsuarioId || p.usuarioId}`;
            const fPrestamo = p.FechaPrestamo || p.fechaPrestamo ? new Date(p.FechaPrestamo || p.fechaPrestamo).toLocaleDateString('es-ES') : '-';
            const fVenc = p.FechaVencimiento || p.fechaVencimiento ? new Date(p.FechaVencimiento || p.fechaVencimiento).toLocaleDateString('es-ES') : '-';
            const estado = p.Estado || p.estado || 'Activo';
            const fv = new Date(p.FechaVencimiento || p.fechaVencimiento);
            const hoy = new Date();
            const dias = Math.ceil((fv - hoy)/(1000*60*60*24));
            let cardClass='prestamo-card', badge='bg-success';
            if (estado==='Devuelto'){ cardClass+=' devuelto'; badge='bg-secondary'; }
            else if (dias<0){ cardClass+=' vencido'; badge='bg-danger'; }
            else if (dias<=3){ cardClass+=' por-vencer'; badge='bg-warning'; }
            html += `
              <div class="col-lg-6 col-xl-4">
                <div class="card ${cardClass} h-100">
                  <div class="card-body">
                    <div class="prestamo-header">
                      <div class="prestamo-info">
                        <div class="prestamo-titulo"><i class="fas fa-book me-2"></i>${libro}</div>
                        <div class="prestamo-usuario"><i class="fas fa-user me-2"></i>${usuario}</div>
                        <div class="prestamo-fechas">
                          <div class="prestamo-fecha"><i class="fas fa-calendar-plus"></i><span>Prestado: ${fPrestamo}</span></div>
                          <div class="prestamo-fecha"><i class="fas fa-calendar-check"></i><span>Vence: ${fVenc}</span></div>
                        </div>
                        ${(p.Observaciones || p.observaciones)?`<div class="prestamo-observaciones"><i class="fas fa-sticky-note me-1"></i><strong>Observaciones:</strong> ${p.Observaciones||p.observaciones}</div>`:''}
                      </div>
                      <div class="prestamo-estado">
                        <span class="badge ${badge}">${estado}</span>
                        ${dias<0 && estado==='Activo'?`<div class="text-danger small mt-1"><i class="fas fa-exclamation-triangle"></i> ${Math.abs(dias)} días de retraso</div>`:
                          dias<=3 && dias>=0 && estado==='Activo'?`<div class="text-warning small mt-1"><i class="fas fa-clock"></i> Vence en ${dias} días</div>`:''}
                      </div>
                    </div>
                  </div>
                </div>
              </div>`;
        });
        html += '</div>';
        container.innerHTML = html;
    }

    mostrarPrestamosEnTabla(prestamos) {
        const container = document.getElementById('lista-prestamos');
        if (!container) return;
        if (!prestamos || prestamos.length === 0) {
            container.innerHTML = `
              <div class="empty-state text-center py-5">
                <i class="fas fa-table fa-4x text-muted mb-3"></i>
                <h4>No hay préstamos para mostrar</h4>
                <p class="text-muted">La tabla se mostrará cuando haya préstamos registrados</p>
              </div>`;
            return;
        }
        let html = `<div class="table-responsive"><table class="table prestamo-table">
          <thead><tr><th>Libro</th><th>Usuario</th><th>Fechas</th><th>Estado</th><th>Observaciones</th></tr></thead><tbody>`;
        prestamos.forEach(p=>{
            const libro = p.LibroTitulo || p.libroTitulo || `Libro ${p.LibroId || p.libroId}`;
            const usuario = p.UsuarioNombre || p.usuarioNombre || `Usuario ${p.UsuarioId || p.usuarioId}`;
            const fPrestamo = p.FechaPrestamo || p.fechaPrestamo ? new Date(p.FechaPrestamo || p.fechaPrestamo).toLocaleDateString('es-ES') : '-';
            const fVenc = p.FechaVencimiento || p.fechaVencimiento ? new Date(p.FechaVencimiento || p.fechaVencimiento).toLocaleDateString('es-ES') : '-';
            const estado = p.Estado || p.estado || 'Activo';
            const fv = new Date(p.FechaVencimiento || p.fechaVencimiento);
            const hoy = new Date();
            const dias = Math.ceil((fv - hoy)/(1000*60*60*24));
            let row='', badge='bg-success';
            if (estado==='Devuelto'){ row='table-secondary'; badge='bg-secondary'; }
            else if (dias<0){ row='table-danger'; badge='bg-danger'; }
            else if (dias<=3){ row='table-warning'; badge='bg-warning'; }
            html += `<tr class="${row}">
              <td><strong>${libro}</strong><br><small class="text-muted">ISBN: ${p.LibroIsbn||p.libroIsbn||'N/A'}</small></td>
              <td><strong>${usuario}</strong><br><small class="text-muted">Cédula: ${p.UsuarioCedula||p.usuarioCedula||'N/A'}</small></td>
              <td><div><strong>Prestado:</strong> ${fPrestamo}</div><div><strong>Vence:</strong> ${fVenc}</div></td>
              <td><span class="badge ${badge}">${estado}</span>
                ${dias<0 && estado==='Activo'?`<div class="text-danger small mt-1">${Math.abs(dias)} días de retraso</div>`:
                  dias<=3 && dias>=0 && estado==='Activo'?`<div class="text-warning small mt-1">Vence en ${dias} días</div>`:''}
              </td>
              <td>${(p.Observaciones||p.observaciones)?`<small>${p.Observaciones||p.observaciones}</small>`:'<span class="text-muted">-</span>'}</td>
            </tr>`;
        });
        html+='</tbody></table></div>';
        container.innerHTML = html;
    }

    // Gestión de devoluciones -------------------------------------
    async cargarPrestamosActivos() {
        try {
            const r = await fetch(PRESTAMOS_CONFIG.urls.prestamosActivos, { headers: { 'Accept':'application/json' } });
            const raw = await r.text();
            let datos; try { datos = JSON.parse(raw); } catch { }
            if (!r.ok) {
                document.getElementById('prestamosActivosContainer').innerHTML =
                  `<div class="alert alert-danger">Error activos: ${(datos&&(datos.message||datos.mensaje))||raw}</div>`;
                return;
            }
            this.mostrarPrestamosActivos(datos);
        } catch(err) {
            document.getElementById('prestamosActivosContainer').innerHTML =
              `<div class="alert alert-danger">Error de conexión: ${err.message}</div>`;
        }
    }

    mostrarPrestamosActivos(prestamos) {
        const cont = document.getElementById('prestamosActivosContainer');
        if (!prestamos || prestamos.length === 0) {
            cont.innerHTML = '<div class="alert alert-info">No hay préstamos activos</div>';
            return;
        }

        let html = `<div class="table-responsive"><table class="table table-striped table-hover">
          <thead class="table-dark"><tr>
            <th>Libro</th><th>Usuario</th><th>Fecha Préstamo</th><th>Fecha Vencimiento</th><th>Estado</th><th>Acciones</th>
          </tr></thead><tbody>`;

        prestamos.forEach(p => {
            const id = this.getPrestamoId(p);
            const idNum = /^\d+$/.test(String(id));
            const idOnClick = idNum ? id : `'${id}'`;

            const fP = new Date(p.FechaPrestamo || p.fechaPrestamo).toLocaleDateString();
            const fvDate = new Date(p.FechaVencimiento || p.fechaVencimiento);
            const fV = isNaN(fvDate.getTime()) ? '-' : fvDate.toLocaleDateString();
            const fvISO = isNaN(fvDate.getTime()) ? new Date().toISOString().slice(0,10) : fvDate.toISOString().slice(0,10);
            const retraso = !isNaN(fvDate.getTime()) && new Date() > fvDate;

            html += `<tr class="${retraso ? 'table-warning' : ''}">
              <td><strong>${p.LibroTitulo || p.libroTitulo || 'N/A'}</strong><br>
                  <small class="text-muted">ISBN: ${p.LibroIsbn || p.libroIsbn || 'N/A'}</small></td>
              <td>${p.UsuarioNombre || p.usuarioNombre || 'N/A'}<br>
                  <small class="text-muted">Cédula: ${p.UsuarioCedula || p.usuarioCedula || 'N/A'}</small></td>
              <td>${fP}</td>
              <td>${fV}</td>
              <td>${retraso ? '<span class="badge bg-danger">En retraso</span>' : '<span class="badge bg-success">Al día</span>'}</td>
              <td>
                ${id ? `<div class="d-flex">
                    <button class="btn btn-success btn-sm" onclick="prestamosManager.registrarDevolucion(${idOnClick})">
                      <i class="fas fa-check me-1"></i>Devolver
                    </button>
                    <button class="btn btn-primary btn-sm ms-2" onclick="prestamosManager.abrirModalRenovar(${idOnClick}, '${fvISO}')">
                      <i class="fas fa-redo me-1"></i>Renovar
                    </button>
                  </div>` : '<span class="text-muted">ID inválido</span>'}
              </td>
            </tr>`;
        });

        html += `</tbody></table></div>`;
        cont.innerHTML = html;
    }

    async registrarDevolucion(idPrestamo) {
        const id = Number(idPrestamo);
        if (!Number.isFinite(id) || id<=0){ showError('ID inválido'); return; }
        if (!confirm('¿Confirmar devolución?')) return;
        try {
            const r = await fetch(`${PRESTAMOS_CONFIG.urls.devolucion}?idPrestamo=${id}`, { method:'PUT' });
            const raw = await r.text(); let j; try { j=JSON.parse(raw);} catch {}
            if (r.ok && j?.success){
                showSuccess(j.mensaje || 'Devuelto');
                this.cargarPrestamosActivos();
                this.cargarPrestamos();
            } else {
                showError(j?.message || raw || 'Error al devolver');
            }
        } catch(err){
            showError('Error conexión: '+err.message);
        }
    }

    toggleSeccionDevoluciones() {
        const s = document.getElementById('seccionDevoluciones');
        if (!s) return;
        if (s.style.display==='none'){
            s.style.display='block';
            this.cargarPrestamosActivos();
        } else {
            s.style.display='none';
        }
    }

    async handleFormSubmit(e) {
        e.preventDefault();
        const form = e.target;
        const fd = new FormData(form);
        try {
            const r = await fetch(PRESTAMOS_CONFIG.urls.create, { method:'POST', body: fd });
            const j = await r.json();
            if (j.success){
                form.reset();
                bootstrap.Modal.getInstance(document.getElementById('modalNuevoPrestamo'))?.hide();
                showSuccess(j.message || 'Préstamo creado');
                this.cargarPrestamos();
                if (document.getElementById('seccionDevoluciones')?.style.display==='block')
                    this.cargarPrestamosActivos();
            } else {
                showError(j.message || 'Error al crear');
            }
        } catch(err){
            showError('Error conexión: '+err.message);
        }
    }

    abrirModalRenovar(idPrestamo, fechaActualVenc) {
        const inputId = document.getElementById('renovarIdPrestamo');
        const inputFecha = document.getElementById('renovarFechaVencimiento');
        if (!inputId || !inputFecha) {
            showError('Modal de renovación no disponible.');
            return;
        }
        inputId.value = idPrestamo;
        const hoyISO = new Date().toISOString().substring(0,10);
        inputFecha.min = fechaActualVenc;
        inputFecha.value = fechaActualVenc > hoyISO ? fechaActualVenc : hoyISO;
        const modalEl = document.getElementById('modalRenovarPrestamo');
        const modal = new bootstrap.Modal(modalEl);
        modal.show();
    }

    // ✅ MEJORAR: Método de renovación con mejor manejo de errores
    async renovarPrestamoSubmit(e) {
        e.preventDefault();
        
        const idPrestamo = document.getElementById('renovarIdPrestamo').value;
        const nuevaFecha = document.getElementById('renovarFechaVencimiento').value;
        
        if (!idPrestamo || !nuevaFecha) {
            showError('Datos incompletos.');
            return;
        }

        // Validar que la fecha sea futura
        const fechaSeleccionada = new Date(nuevaFecha);
        const hoy = new Date();
        hoy.setHours(0, 0, 0, 0);
        
        if (fechaSeleccionada <= hoy) {
            showError('La nueva fecha debe ser posterior a hoy');
            return;
        }

        // Deshabilitar botón durante la operación
        const btnSubmit = e.target.querySelector('button[type="submit"]');
        const textoOriginal = btnSubmit ? btnSubmit.innerHTML : '';
        if (btnSubmit) {
            btnSubmit.disabled = true;
            btnSubmit.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Renovando...';
        }

        try {
            // ✅ Enviar con query parameters como antes
            const url = `${PRESTAMOS_CONFIG.urls.renovar}?idPrestamo=${encodeURIComponent(idPrestamo)}&nuevaFechaVencimiento=${encodeURIComponent(nuevaFecha)}`;
            
            const response = await fetch(url, { 
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                }
            });
            
            const text = await response.text();
            console.log('Respuesta del servidor:', text); // ✅ Para debugging
            
            let data;
            try { 
                data = JSON.parse(text); 
            } catch (parseError) {
                console.error('Error al parsear JSON:', parseError);
                showError('Error en la respuesta del servidor');
                return;
            }

            if (response.ok && data?.success) {
                showSuccess(data.mensaje || 'Préstamo renovado correctamente');
                
                // Cerrar modal
                const modalElement = document.getElementById('modalRenovarPrestamo');
                const modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) modal.hide();
                
                // Recargar datos
                await this.cargarPrestamosActivos();
                await this.cargarPrestamos();
            } else {
                // Mostrar mensaje de error específico
                const errorMsg = data?.message || data?.mensaje || text || 'Error al renovar préstamo';
                showError(errorMsg);
            }
        } catch (err) {
            console.error('Error de conexión:', err);
            showError('Error de conexión: ' + err.message);
        } finally {
            // Rehabilitar botón
            if (btnSubmit) {
                btnSubmit.disabled = false;
                btnSubmit.innerHTML = textoOriginal;
            }
        }
    }
}

// Inicialización
let prestamosManager;
document.addEventListener('DOMContentLoaded', ()=>{ prestamosManager = new PrestamosManager(); });