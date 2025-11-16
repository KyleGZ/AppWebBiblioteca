// Configuración de las URLs usando helper de ASP.NET Core
const PRESTAMOS_CONFIG = {
    baseUrl: window.location.origin,
    urls: {
        prestamos: '/Prestamos/GetPrestamos',
        prestamosActivos: '/Prestamos/GetPrestamosActivos',
        devolucion: '/Prestamos/RegistrarDevolucion',
        create: '/Prestamos/Create'
    }
};

// Clase para manejar préstamos con vista mejorada
class PrestamosManager {
    constructor() {
        console.log('Inicializando PrestamosManager...');
        this.vistaActual = 'tarjetas'; // 'tarjetas' o 'tabla'
        this.prestamosData = [];
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.cargarPrestamos();
    }

    setupEventListeners() {
        // Configurar botón de devolución
        const btnDevolucion = document.getElementById('btnRegistrarDevolucion');
        if (btnDevolucion) {
            btnDevolucion.addEventListener('click', () => this.toggleSeccionDevoluciones());
        }

        // Configurar formulario
        const form = document.getElementById('formNuevoPrestamo');
        if (form) {
            form.addEventListener('submit', (e) => this.handleFormSubmit(e));
        }

        // Configurar toggles de vista
        document.getElementById('vista-tarjetas').addEventListener('click', () => this.cambiarVista('tarjetas'));
        document.getElementById('vista-tabla').addEventListener('click', () => this.cambiarVista('tabla'));

        // Configurar búsqueda y filtros
        document.getElementById('searchInput').addEventListener('input', (e) => this.filtrarPrestamos());
        document.getElementById('filtroEstado').addEventListener('change', (e) => this.filtrarPrestamos());
        document.getElementById('btnLimpiarFiltros').addEventListener('click', () => this.limpiarFiltros());
    }

    async cargarPrestamos() {
        try {
            const response = await fetch(PRESTAMOS_CONFIG.urls.prestamos);
            if (!response.ok) throw new Error('Error del servidor: ' + response.status);

            const prestamos = await response.json();
            this.prestamosData = prestamos;
            this.actualizarEstadisticas(prestamos);
            this.mostrarPrestamos(prestamos);
        } catch (error) {
            console.error("Error cargando préstamos:", error);
            document.getElementById('lista-prestamos').innerHTML =
                `<div class="alert alert-danger">
                    <i class="fas fa-exclamation-triangle me-2"></i>
                    Error al cargar préstamos: ${error.message}
                </div>`;
        }
    }

    actualizarEstadisticas(prestamos) {
        const hoy = new Date();
        const stats = {
            total: prestamos.length,
            activos: prestamos.filter(p => p.Estado === 'Activo' || p.estado === 'Activo').length,
            vencidos: prestamos.filter(p => {
                const fechaVenc = new Date(p.FechaVencimiento || p.fechaVencimiento);
                return (p.Estado === 'Activo' || p.estado === 'Activo') && fechaVenc < hoy;
            }).length,
            devueltosHoy: prestamos.filter(p => {
                const fechaDev = new Date(p.FechaDevolucion || p.fechaDevolucion);
                return (p.Estado === 'Devuelto' || p.estado === 'Devuelto') && 
                       fechaDev.toDateString() === hoy.toDateString();
            }).length
        };

        document.getElementById('total-prestamos').textContent = stats.total;
        document.getElementById('prestamos-activos').textContent = stats.activos;
        document.getElementById('prestamos-vencidos').textContent = stats.vencidos;
        document.getElementById('devoluciones-hoy').textContent = stats.devueltosHoy;
    }

    cambiarVista(vista) {
        this.vistaActual = vista;
        
        // Actualizar botones
        document.getElementById('vista-tarjetas').classList.toggle('active', vista === 'tarjetas');
        document.getElementById('vista-tabla').classList.toggle('active', vista === 'tabla');
        
        // Mostrar préstamos en la nueva vista
        this.mostrarPrestamos(this.prestamosData);
    }

    mostrarPrestamos(prestamos) {
        if (this.vistaActual === 'tarjetas') {
            this.mostrarPrestamosEnTarjetas(prestamos);
        } else {
            this.mostrarPrestamosEnTabla(prestamos);
        }
    }

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
                </div>
            `;
            return;
        }

        let html = '<div class="row g-3">';
        prestamos.forEach(prestamo => {
            const libroInfo = prestamo.LibroTitulo || prestamo.libroTitulo || `Libro ${prestamo.LibroId || prestamo.libroId}`;
            const usuarioInfo = prestamo.UsuarioNombre || prestamo.usuarioNombre || `Usuario ${prestamo.UsuarioId || prestamo.usuarioId}`;
            const fechaPrestamo = prestamo.FechaPrestamo || prestamo.fechaPrestamo ? 
                new Date(prestamo.FechaPrestamo || prestamo.fechaPrestamo).toLocaleDateString('es-ES') : 'Fecha no disponible';
            const fechaVencimiento = prestamo.FechaVencimiento || prestamo.fechaVencimiento ? 
                new Date(prestamo.FechaVencimiento || prestamo.fechaVencimiento).toLocaleDateString('es-ES') : 'Fecha no disponible';
            
            const estado = prestamo.Estado || prestamo.estado || 'Activo';
            const fechaVenc = new Date(prestamo.FechaVencimiento || prestamo.fechaVencimiento);
            const hoy = new Date();
            const diasParaVencer = Math.ceil((fechaVenc - hoy) / (1000 * 60 * 60 * 24));
            
            let cardClass = 'prestamo-card';
            let estadoBadge = 'bg-success';
            
            if (estado === 'Devuelto') {
                cardClass += ' devuelto';
                estadoBadge = 'bg-secondary';
            } else if (diasParaVencer < 0) {
                cardClass += ' vencido';
                estadoBadge = 'bg-danger';
            } else if (diasParaVencer <= 3) {
                cardClass += ' por-vencer';
                estadoBadge = 'bg-warning';
            }

            html += `
                <div class="col-lg-6 col-xl-4">
                    <div class="card ${cardClass} h-100">
                        <div class="card-body">
                            <div class="prestamo-header">
                                <div class="prestamo-info">
                                    <div class="prestamo-titulo">
                                        <i class="fas fa-book me-2"></i>${libroInfo}
                                    </div>
                                    <div class="prestamo-usuario">
                                        <i class="fas fa-user me-2"></i>${usuarioInfo}
                                    </div>
                                    <div class="prestamo-fechas">
                                        <div class="prestamo-fecha">
                                            <i class="fas fa-calendar-plus"></i>
                                            <span>Prestado: ${fechaPrestamo}</span>
                                        </div>
                                        <div class="prestamo-fecha">
                                            <i class="fas fa-calendar-check"></i>
                                            <span>Vence: ${fechaVencimiento}</span>
                                        </div>
                                    </div>
                                    ${(prestamo.Observaciones || prestamo.observaciones) ? `
                                        <div class="prestamo-observaciones">
                                            <i class="fas fa-sticky-note me-1"></i>
                                            <strong>Observaciones:</strong> ${prestamo.Observaciones || prestamo.observaciones}
                                        </div>
                                    ` : ''}
                                </div>
                                <div class="prestamo-estado">
                                    <span class="badge ${estadoBadge}">${estado}</span>
                                    ${diasParaVencer < 0 && estado === 'Activo' ? `
                                        <div class="text-danger small mt-1">
                                            <i class="fas fa-exclamation-triangle"></i>
                                            ${Math.abs(diasParaVencer)} días de retraso
                                        </div>
                                    ` : diasParaVencer <= 3 && diasParaVencer >= 0 && estado === 'Activo' ? `
                                        <div class="text-warning small mt-1">
                                            <i class="fas fa-clock"></i>
                                            Vence en ${diasParaVencer} días
                                        </div>
                                    ` : ''}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
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
                </div>
            `;
            return;
        }

        let html = `
            <div class="table-responsive">
                <table class="table prestamo-table">
                    <thead>
                        <tr>
                            <th><i class="fas fa-book me-1"></i>Libro</th>
                            <th><i class="fas fa-user me-1"></i>Usuario</th>
                            <th><i class="fas fa-calendar me-1"></i>Fechas</th>
                            <th><i class="fas fa-info-circle me-1"></i>Estado</th>
                            <th><i class="fas fa-sticky-note me-1"></i>Observaciones</th>
                        </tr>
                    </thead>
                    <tbody>
        `;

        prestamos.forEach(prestamo => {
            const libroInfo = prestamo.LibroTitulo || prestamo.libroTitulo || `Libro ${prestamo.LibroId || prestamo.libroId}`;
            const usuarioInfo = prestamo.UsuarioNombre || prestamo.usuarioNombre || `Usuario ${prestamo.UsuarioId || prestamo.usuarioId}`;
            const fechaPrestamo = prestamo.FechaPrestamo || prestamo.fechaPrestamo ? 
                new Date(prestamo.FechaPrestamo || prestamo.fechaPrestamo).toLocaleDateString('es-ES') : '-';
            const fechaVencimiento = prestamo.FechaVencimiento || prestamo.fechaVencimiento ? 
                new Date(prestamo.FechaVencimiento || prestamo.fechaVencimiento).toLocaleDateString('es-ES') : '-';
            
            const estado = prestamo.Estado || prestamo.estado || 'Activo';
            const fechaVenc = new Date(prestamo.FechaVencimiento || prestamo.fechaVencimiento);
            const hoy = new Date();
            const diasParaVencer = Math.ceil((fechaVenc - hoy) / (1000 * 60 * 60 * 24));
            
            let rowClass = '';
            let estadoBadge = 'bg-success';
            
            if (estado === 'Devuelto') {
                rowClass = 'table-secondary';
                estadoBadge = 'bg-secondary';
            } else if (diasParaVencer < 0) {
                rowClass = 'table-danger';
                estadoBadge = 'bg-danger';
            } else if (diasParaVencer <= 3) {
                rowClass = 'table-warning';
                estadoBadge = 'bg-warning';
            }

            html += `
                <tr class="${rowClass}">
                    <td>
                        <strong>${libroInfo}</strong><br>
                        <small class="text-muted">ISBN: ${prestamo.LibroIsbn || prestamo.libroIsbn || 'N/A'}</small>
                    </td>
                    <td>
                        <strong>${usuarioInfo}</strong><br>
                        <small class="text-muted">Cédula: ${prestamo.UsuarioCedula || prestamo.usuarioCedula || 'N/A'}</small>
                    </td>
                    <td>
                        <div><strong>Prestado:</strong> ${fechaPrestamo}</div>
                        <div><strong>Vence:</strong> ${fechaVencimiento}</div>
                    </td>
                    <td>
                        <span class="badge ${estadoBadge}">${estado}</span>
                        ${diasParaVencer < 0 && estado === 'Activo' ? `
                            <div class="text-danger small mt-1">
                                ${Math.abs(diasParaVencer)} días de retraso
                            </div>
                        ` : diasParaVencer <= 3 && diasParaVencer >= 0 && estado === 'Activo' ? `
                            <div class="text-warning small mt-1">
                                Vence en ${diasParaVencer} días
                            </div>
                        ` : ''}
                    </td>
                    <td>
                        ${(prestamo.Observaciones || prestamo.observaciones) ? 
                            `<small>${prestamo.Observaciones || prestamo.observaciones}</small>` : 
                            '<span class="text-muted">-</span>'}
                    </td>
                </tr>
            `;
        });

        html += `
                    </tbody>
                </table>
            </div>
        `;

        container.innerHTML = html;
    }

    filtrarPrestamos() {
        const busqueda = document.getElementById('searchInput').value.toLowerCase();
        const filtroEstado = document.getElementById('filtroEstado').value;
        
        let prestamosFiltrados = this.prestamosData.filter(prestamo => {
            const coincideBusqueda = !busqueda || 
                (prestamo.LibroTitulo || prestamo.libroTitulo || '').toLowerCase().includes(busqueda) ||
                (prestamo.UsuarioNombre || prestamo.usuarioNombre || '').toLowerCase().includes(busqueda) ||
                (prestamo.LibroIsbn || prestamo.libroIsbn || '').toLowerCase().includes(busqueda);
            
            const coincideEstado = !filtroEstado || 
                (prestamo.Estado || prestamo.estado) === filtroEstado;
            
            return coincideBusqueda && coincideEstado;
        });
        
        this.mostrarPrestamos(prestamosFiltrados);
    }

    limpiarFiltros() {
        document.getElementById('searchInput').value = '';
        document.getElementById('filtroEstado').value = '';
        this.mostrarPrestamos(this.prestamosData);
    }

    async cargarPrestamosActivos() {
        try {
            const response = await fetch(PRESTAMOS_CONFIG.urls.prestamosActivos);
            if (response.ok) {
                const prestamos = await response.json();
                this.mostrarPrestamosActivos(prestamos);
            } else {
                document.getElementById('prestamosActivosContainer').innerHTML =
                    '<div class="alert alert-danger">Error al cargar préstamos activos</div>';
            }
        } catch (error) {
            console.error('Error:', error);
            document.getElementById('prestamosActivosContainer').innerHTML =
                '<div class="alert alert-danger">Error de conexión: ' + error.message + '</div>';
        }
    }

    mostrarPrestamosActivos(prestamos) {
        const contenedor = document.getElementById('prestamosActivosContainer');
        if (!prestamos || prestamos.length === 0) {
            contenedor.innerHTML = '<div class="alert alert-info">No hay préstamos activos para gestionar</div>';
            return;
        }

        let html = `
            <div class="table-responsive">
                <table class="table table-striped table-hover">
                    <thead class="table-dark">
                        <tr>
                            <th>Libro</th>
                            <th>Usuario</th>
                            <th>Fecha Préstamo</th>
                            <th>Fecha Vencimiento</th>
                            <th>Estado</th>
                            <th>Acciones</th>
                        </tr>
                    </thead>
                    <tbody>
        `;

        prestamos.forEach(prestamo => {
            const fechaPrestamo = new Date(prestamo.FechaPrestamo || prestamo.fechaPrestamo).toLocaleDateString();
            const fechaVencimiento = new Date(prestamo.FechaVencimiento || prestamo.fechaVencimiento).toLocaleDateString();
            const hoy = new Date();
            const fechaVence = new Date(prestamo.FechaVencimiento || prestamo.fechaVencimiento);
            const tieneRetraso = hoy > fechaVence;

            html += `
                <tr class="${tieneRetraso ? 'table-warning' : ''}">
                    <td>
                        <strong>${prestamo.LibroTitulo || prestamo.libroTitulo || 'N/A'}</strong><br>
                        <small class="text-muted">ISBN: ${prestamo.LibroIsbn || prestamo.libroIsbn || 'N/A'}</small>
                    </td>
                    <td>
                        ${prestamo.UsuarioNombre || prestamo.usuarioNombre || 'N/A'}<br>
                        <small class="text-muted">Cédula: ${prestamo.UsuarioCedula || prestamo.usuarioCedula || 'N/A'}</small>
                    </td>
                    <td>${fechaPrestamo}</td>
                    <td>${fechaVencimiento}</td>
                    <td>
                        ${tieneRetraso ?
                            `<span class="badge bg-danger">En retraso</span>` :
                            `<span class="badge bg-success">Al día</span>`
                        }
                    </td>
                    <td>
                        <button class="btn btn-success btn-sm" onclick="prestamosManager.registrarDevolucion(${prestamo.Id || prestamo.id})">
                            <i class="fas fa-check me-1"></i>Registrar Devolución
                        </button>
                    </td>
                </tr>
            `;
        });

        html += `
                    </tbody>
                </table>
            </div>
        `;

        contenedor.innerHTML = html;
    }

    async registrarDevolucion(idPrestamo) {
        if (!confirm('¿Estás seguro de que deseas registrar la devolución de este libro?')) {
            return;
        }

        try {
            const response = await fetch(PRESTAMOS_CONFIG.urls.devolucion + `?id=${idPrestamo}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const resultado = await response.json();
                showSuccess(resultado.mensaje || 'Devolución registrada exitosamente');
                
                // Recargar todo
                this.cargarPrestamosActivos();
                this.cargarPrestamos();
            } else {
                const error = await response.json();
                showError(error.message || 'No se pudo registrar la devolución');
            }
        } catch (error) {
            console.error('Error:', error);
            showError('Error de conexión al registrar la devolución');
        }
    }

    toggleSeccionDevoluciones() {
        const seccion = document.getElementById('seccionDevoluciones');
        if (seccion.style.display === 'none') {
            seccion.style.display = 'block';
            this.cargarPrestamosActivos();
        } else {
            seccion.style.display = 'none';
        }
    }

    async handleFormSubmit(e) {
        e.preventDefault();
        const form = e.target;

        const formData = new FormData(form);
        
        try {
            const response = await fetch(PRESTAMOS_CONFIG.urls.create, {
                method: 'POST',
                body: formData
            });

            const data = await response.json();

            if (data.success) {
                form.reset();
                const modal = bootstrap.Modal.getInstance(document.getElementById('modalNuevoPrestamo'));
                if (modal) modal.hide();

                showSuccess('Préstamo registrado exitosamente');
                this.cargarPrestamos();
                this.cargarPrestamosActivos();
            } else {
                showError(data.message || 'No se pudo crear el préstamo');
            }
        } catch (error) {
            console.error('Error:', error);
            showError('Error de conexión: ' + error.message);
        }
    }
}

// Inicialización global
let prestamosManager;
document.addEventListener('DOMContentLoaded', function() {
    prestamosManager = new PrestamosManager();
});