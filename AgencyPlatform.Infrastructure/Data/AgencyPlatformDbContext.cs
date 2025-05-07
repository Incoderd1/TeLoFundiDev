using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AgencyPlatform.Core.Entities;

public partial class AgencyPlatformDbContext : DbContext
{
    public AgencyPlatformDbContext()
    {
    }

    public AgencyPlatformDbContext(DbContextOptions<AgencyPlatformDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<acciones_punto> acciones_puntos { get; set; }

    public virtual DbSet<acompanante> acompanantes { get; set; }

    public virtual DbSet<acompanante_categoria> acompanante_categorias { get; set; }

    public virtual DbSet<agencia> agencias { get; set; }

    public virtual DbSet<anuncios_destacado> anuncios_destacados { get; set; }

    public virtual DbSet<auth_proveedore> auth_proveedores { get; set; }

    public virtual DbSet<categoria> categorias { get; set; }

    public virtual DbSet<cliente> clientes { get; set; }

    public virtual DbSet<compras_paquete> compras_paquetes { get; set; }

    public virtual DbSet<contacto> contactos { get; set; }

    public virtual DbSet<cupones_cliente> cupones_clientes { get; set; }

    public virtual DbSet<foto> fotos { get; set; }

    public virtual DbSet<intentos_login> intentos_logins { get; set; }

    public virtual DbSet<membresias_vip> membresias_vips { get; set; }

    public virtual DbSet<movimientos_punto> movimientos_puntos { get; set; }

    public virtual DbSet<paquete_cupones_detalle> paquete_cupones_detalles { get; set; }

    public virtual DbSet<paquetes_cupone> paquetes_cupones { get; set; }

    public virtual DbSet<participantes_sorteo> participantes_sorteos { get; set; }

    public virtual DbSet<refresh_token> refresh_tokens { get; set; }

    public virtual DbSet<role> roles { get; set; }

    public virtual DbSet<servicio> servicios { get; set; }

    public virtual DbSet<sorteo> sorteos { get; set; }

    public virtual DbSet<suscripciones_vip> suscripciones_vips { get; set; }

    public virtual DbSet<tipos_cupone> tipos_cupones { get; set; }

    public virtual DbSet<tokens_reset_password> tokens_reset_passwords { get; set; }

    public virtual DbSet<usuario> usuarios { get; set; }

    public virtual DbSet<usuario_auth_externo> usuario_auth_externos { get; set; }

    public virtual DbSet<verificacione> verificaciones { get; set; }

    public virtual DbSet<visitas_perfil> visitas_perfils { get; set; }

    public virtual DbSet<vw_agencias_acompanante> vw_agencias_acompanantes { get; set; }

    public virtual DbSet<vw_clientes_info> vw_clientes_infos { get; set; }

    public virtual DbSet<vw_perfiles_destacado> vw_perfiles_destacados { get; set; }

    public virtual DbSet<vw_perfiles_populare> vw_perfiles_populares { get; set; }

    public virtual DbSet<vw_perfiles_reciente> vw_perfiles_recientes { get; set; }

    public virtual DbSet<vw_ranking_perfile> vw_ranking_perfiles { get; set; }

    public virtual DbSet<failed_login_attempt> failed_login_attempt { get; set; }
    public virtual DbSet<intentos_login> intentos_login { get; set; }

    public virtual DbSet<SolicitudAgencia> SolicitudAgencias { get; set; }


    public virtual DbSet<Comision> Comisiones { get; set; }
    public DbSet<solicitud_agencia> solicitud_agencias { get; set; } // Cambiado a plural

    public DbSet<solicitud_registro_agencia> SolicitudesRegistroAgencia { get; set; }

    public DbSet<movimientos_puntos_agencia> MovimientosPuntosAgencia { get; set; }

    public DbSet<pago_verificacion> PagosVerificacion { get; set; }


    //mapearlas

    public DbSet<busqueda_guardada> busqueda_guardadas { get; set; }
    public DbSet<registro_busqueda> registro_busquedas { get; set; }
    public DbSet<transaccion> transacciones { get; set; }
    public DbSet<transferencia> transferencias { get; set; }






    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=PlataformaAgencia;Username=postgres;Password=C123456");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<intentos_login>(entity =>
        {
            entity.ToTable("intentos_login", "plataforma");
            entity.HasKey(e => e.id).HasName("intentos_login_pkey");

            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.ip_address).HasMaxLength(50);
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
        // Dentro del método OnModelCreating
        modelBuilder.Entity<busqueda_guardada>(entity =>
        {
            entity.ToTable("busqueda_guardada", "plataforma");

            entity.HasKey(e => e.id);
            entity.Property(e => e.id).UseIdentityAlwaysColumn();

            entity.Property(e => e.usuario_id).IsRequired();
            entity.Property(e => e.nombre).HasMaxLength(255).IsRequired();
            entity.Property(e => e.criterios_json).IsRequired();
            entity.Property(e => e.fecha_creacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.veces_usada).HasDefaultValue(1);

            // Relación con usuario
            entity.HasOne(d => d.usuario)
                .WithMany()
                .HasForeignKey(d => d.usuario_id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<registro_busqueda>(entity =>
        {
            entity.ToTable("registro_busqueda", "plataforma");

            entity.HasKey(e => e.id);
            entity.Property(e => e.id).UseIdentityAlwaysColumn();

            entity.Property(e => e.fecha_busqueda).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.criterios_json).IsRequired();
            entity.Property(e => e.cantidad_resultados).IsRequired();
            entity.Property(e => e.ip_busqueda).HasMaxLength(50);
            entity.Property(e => e.user_agent).HasMaxLength(255);

            // Relación con usuario (opcional)
            entity.HasOne(d => d.usuario)
                .WithMany()
                .HasForeignKey(d => d.usuario_id)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<pago_verificacion>(entity =>
        {
            entity.ToTable("pagos_verificacion", "plataforma");

            entity.HasKey(e => e.id);

            entity.Property(e => e.id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.verificacion_id)
                .HasColumnName("verificacion_id")
                .IsRequired();

            entity.Property(e => e.acompanante_id)
                .HasColumnName("acompanante_id")
                .IsRequired();

            entity.Property(e => e.agencia_id)
                .HasColumnName("agencia_id")
                .IsRequired();

            entity.Property(e => e.monto)
                .HasColumnName("monto")
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            entity.Property(e => e.moneda)
                .HasColumnName("moneda")
                .HasMaxLength(10)
                .HasDefaultValue("USD");

            entity.Property(e => e.metodo_pago)
                .HasColumnName("metodo_pago")
                .HasMaxLength(50);

            entity.Property(e => e.referencia_pago)
                .HasColumnName("referencia_pago")
                .HasMaxLength(255);

            entity.Property(e => e.estado)
                .HasColumnName("estado")
                .HasMaxLength(50)
                .HasDefaultValue("pendiente");

            entity.Property(e => e.fecha_pago)
                .HasColumnName("fecha_pago");

            entity.Property(e => e.created_at)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.updated_at)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.verificacion)
                .WithMany()
                .HasForeignKey(d => d.verificacion_id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.acompanante)
                .WithMany()
                .HasForeignKey(d => d.acompanante_id);

            entity.HasOne(d => d.agencia)
                .WithMany()
                .HasForeignKey(d => d.agencia_id);
        });
        modelBuilder.Entity<solicitud_agencia>(entity =>
        {
            entity.ToTable("solicitudes_registro_agencia");

            entity.HasKey(e => e.id);

            entity.Property(e => e.estado)
                .IsRequired()
                .HasDefaultValue("pendiente");

            entity.Property(e => e.fecha_solicitud)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IniciadaPorAgencia).HasColumnName("iniciada_por_agencia").HasDefaultValue(false);


            // Configurar relación con acompanante
            entity.HasOne(e => e.acompanante)
                .WithMany()
                .HasForeignKey(e => e.acompanante_id)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar relación con agencia
            entity.HasOne(e => e.agencia)
                .WithMany()
                .HasForeignKey(e => e.agencia_id)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice único para evitar solicitudes duplicadas
            entity.HasIndex(e => new { e.acompanante_id, e.agencia_id, e.estado })
                .IsUnique();
        });

        modelBuilder.Entity<movimientos_puntos_agencia>(entity =>
        {
            entity.ToTable("movimientos_puntos_agencia", "plataforma");

            entity.HasKey(e => e.id);

            entity.Property(e => e.id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.agencia_id)
                .HasColumnName("agencia_id")
                .IsRequired();

            entity.Property(e => e.cantidad)
                .HasColumnName("cantidad")
                .IsRequired();

            entity.Property(e => e.tipo)
                .HasColumnName("tipo")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.concepto)
                .HasColumnName("concepto")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.saldo_anterior)
                .HasColumnName("saldo_anterior")
                .IsRequired();

            entity.Property(e => e.saldo_nuevo)
                .HasColumnName("saldo_nuevo")
                .IsRequired();

            entity.Property(e => e.fecha)
                .HasColumnName("fecha")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.created_at)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.agencia)
                .WithMany()
                .HasForeignKey(d => d.agencia_id)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<transaccion>(entity =>
        {
            entity.HasKey(e => e.id).HasName("transacciones_pkey");
            entity.ToTable("transacciones", "plataforma");
            entity.Property(e => e.id).UseIdentityAlwaysColumn();

            entity.Property(e => e.monto_total).HasPrecision(10, 2);
            entity.Property(e => e.monto_acompanante).HasPrecision(10, 2);
            entity.Property(e => e.monto_agencia).HasPrecision(10, 2);
            entity.Property(e => e.estado).HasMaxLength(50);
            entity.Property(e => e.proveedor_pago).HasMaxLength(50);
            entity.Property(e => e.id_transaccion_externa).HasMaxLength(255);
            entity.Property(e => e.metadata).HasColumnType("text");
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.concepto).HasMaxLength(255);


            entity.HasOne(d => d.cliente)
                .WithMany()
                .HasForeignKey(d => d.cliente_id)
                .HasConstraintName("transacciones_cliente_id_fkey");

            entity.HasOne(d => d.acompanante)
                .WithMany()
                .HasForeignKey(d => d.acompanante_id)
                .HasConstraintName("transacciones_acompanante_id_fkey");

            entity.HasOne(d => d.agencia)
                .WithMany()
                .HasForeignKey(d => d.agencia_id)
                .HasConstraintName("transacciones_agencia_id_fkey");
        });

        modelBuilder.Entity<transferencia>(entity =>
        {
            entity.HasKey(e => e.id).HasName("transferencias_pkey");
            entity.ToTable("transferencias", "plataforma");
            entity.Property(e => e.id).UseIdentityAlwaysColumn();

            entity.Property(e => e.estado).HasMaxLength(50);
            entity.Property(e => e.proveedor_pago).HasMaxLength(50);
            entity.Property(e => e.id_transferencia_externa).HasMaxLength(255);
            entity.Property(e => e.error_mensaje).HasMaxLength(500);
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.transaccion)
                .WithMany()
                .HasForeignKey(d => d.transaccion_id)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("transferencias_transaccion_id_fkey");
        });


        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<acciones_punto>(entity =>
        {
            entity.HasKey(e => e.id).HasName("acciones_puntos_pkey");

            entity.ToTable("acciones_puntos", "plataforma");

            entity.HasIndex(e => e.nombre, "acciones_puntos_nombre_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.esta_activa).HasDefaultValue(true);
            entity.Property(e => e.nombre).HasMaxLength(100);
            entity.Property(e => e.puntos).HasDefaultValue(1);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<acompanante>(entity =>
        {
            entity.HasKey(e => e.id).HasName("acompanantes_pkey");
            entity.ToTable("acompanantes", "plataforma");
            entity.HasIndex(e => e.usuario_id, "acompanantes_usuario_id_key").IsUnique();
            entity.HasIndex(e => e.ciudad, "idx_acompanantes_ciudad");
            entity.HasIndex(e => e.esta_disponible, "idx_acompanantes_disponible");
            entity.HasIndex(e => e.esta_verificado, "idx_acompanantes_verificado");
            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.ciudad).HasMaxLength(100);
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.esta_disponible).HasDefaultValue(true);
            entity.Property(e => e.esta_verificado).HasDefaultValue(false);
            entity.Property(e => e.genero).HasMaxLength(50);
            entity.Property(e => e.idiomas).HasMaxLength(255);
            entity.Property(e => e.score_actividad).HasDefaultValue(0L);


            entity.Property(e => e.stripe_account_id).HasMaxLength(100);
            entity.Property(e => e.stripe_payouts_enabled).HasDefaultValue(false);
            entity.Property(e => e.stripe_charges_enabled).HasDefaultValue(false);
            entity.Property(e => e.stripe_onboarding_completed).HasDefaultValue(false);




            entity.Property(e => e.latitud);
            entity.Property(e => e.longitud);
            entity.Property(e => e.direccion_completa).HasMaxLength(500);

            // Campos de contacto
            entity.Property(e => e.telefono).HasMaxLength(20);
            entity.Property(e => e.whatsapp).HasMaxLength(20);
            entity.Property(e => e.email_contacto).HasMaxLength(255);

            // Campos de visibilidad con valores predeterminados
            entity.Property(e => e.mostrar_telefono).HasDefaultValue(true);
            entity.Property(e => e.mostrar_whatsapp).HasDefaultValue(true);
            entity.Property(e => e.mostrar_email).HasDefaultValue(true);

            entity.Property(e => e.moneda)
                .HasMaxLength(10)
                .HasDefaultValueSql("'USD'::character varying");
            entity.Property(e => e.nombre_perfil).HasMaxLength(100);
            entity.Property(e => e.pais).HasMaxLength(100);
            entity.Property(e => e.tarifa_base).HasPrecision(10, 2);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(d => d.agencia).WithMany(p => p.acompanantes)
                .HasForeignKey(d => d.agencia_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("acompanantes_agencia_id_fkey");
            entity.HasOne(d => d.usuario).WithOne(p => p.acompanante)
                .HasForeignKey<acompanante>(d => d.usuario_id)
                .HasConstraintName("acompanantes_usuario_id_fkey");
        });
        modelBuilder.Entity<SolicitudAgencia>(entity =>
        {
            entity.ToTable("solicitudes_agencia", "plataforma");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AcompananteId).HasColumnName("acompanante_id");
            entity.Property(e => e.AgenciaId).HasColumnName("agencia_id");
            entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(20).HasDefaultValue("pendiente");
            entity.Property(e => e.FechaSolicitud).HasColumnName("fecha_solicitud").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.FechaRespuesta).HasColumnName("fecha_respuesta");
            entity.Property(e => e.IniciadaPorAgencia).HasColumnName("iniciada_por_agencia").HasDefaultValue(false);


            entity.HasIndex(e => new { e.AcompananteId, e.AgenciaId }).IsUnique();

            entity.HasOne(e => e.Acompanante)
                .WithMany()
                .HasForeignKey(e => e.AcompananteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Agencia)
                .WithMany()
                .HasForeignKey(e => e.AgenciaId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<acompanante_categoria>(entity =>
        {
            entity.HasKey(e => e.id).HasName("acompanante_categorias_pkey");

            entity.ToTable("acompanante_categorias", "plataforma");

            entity.HasIndex(e => new { e.acompanante_id, e.categoria_id }, "acompanante_categorias_acompanante_id_categoria_id_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.acompanante).WithMany(p => p.acompanante_categoria)
                .HasForeignKey(d => d.acompanante_id)
                .HasConstraintName("acompanante_categorias_acompanante_id_fkey");

            entity.HasOne(d => d.categoria).WithMany(p => p.acompanante_categoria)
                .HasForeignKey(d => d.categoria_id)
                .HasConstraintName("acompanante_categorias_categoria_id_fkey");
        });

        modelBuilder.Entity<agencia>(entity =>
        {
            entity.HasKey(e => e.id).HasName("agencias_pkey");

            entity.ToTable("agencias", "plataforma");

            entity.HasIndex(e => e.usuario_id, "agencias_usuario_id_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.ciudad).HasMaxLength(100);
            entity.Property(e => e.comision_porcentaje)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0.00");
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.esta_verificada).HasDefaultValue(false);
            entity.Property(e => e.logo_url).HasMaxLength(512);
            entity.Property(e => e.nombre).HasMaxLength(255);
            entity.Property(e => e.pais).HasMaxLength(100);
            entity.Property(e => e.sitio_web).HasMaxLength(255);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(a => a.email).HasMaxLength(255);
            entity.Property(a => a.puntos_gastados).HasColumnName("puntos_gastados").HasDefaultValue(0);
            entity.Property(a => a.puntos_acumulados).HasColumnName("puntos_acumulados").HasDefaultValue(0);


            entity.Property(a => a.stripe_account_id).HasColumnName("stripe_account_id").HasMaxLength(50).IsRequired(false);



            entity.HasOne(d => d.usuario).WithOne(p => p.agencia)
                .HasForeignKey<agencia>(d => d.usuario_id)
                .HasConstraintName("agencias_usuario_id_fkey");
     
        });
        modelBuilder.Entity<solicitud_registro_agencia>(entity =>
        {
            entity.ToTable("solicitudes_registro_agencia", "plataforma");

            entity.HasKey(e => e.id);

            entity.Property(e => e.id)
                .HasColumnName("id")
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.nombre)
                .IsRequired()
                .HasColumnName("nombre")
                .HasMaxLength(255);

            entity.Property(e => e.email)
                .IsRequired()
                .HasColumnName("email")
                .HasMaxLength(255);

            entity.HasIndex(e => e.email)
                .IsUnique();

            entity.Property(e => e.password_hash)
                .IsRequired()
                .HasColumnName("password_hash")
                .HasMaxLength(255);

            entity.Property(e => e.descripcion)
                .HasColumnName("descripcion");

            entity.Property(e => e.logo_url)
                .HasColumnName("logo_url")
                .HasMaxLength(512);

            entity.Property(e => e.sitio_web)
                .HasColumnName("sitio_web")
                .HasMaxLength(255);

            entity.Property(e => e.direccion)
                .HasColumnName("direccion");

            entity.Property(e => e.ciudad)
                .HasColumnName("ciudad")
                .HasMaxLength(100);

            entity.Property(e => e.pais)
                .HasColumnName("pais")
                .HasMaxLength(100);

            entity.Property(e => e.fecha_solicitud)
                .HasColumnName("fecha_solicitud")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.fecha_respuesta)
                .HasColumnName("fecha_respuesta");

            entity.Property(e => e.estado)
                .HasColumnName("estado")
                .HasMaxLength(50)
                .HasDefaultValue("pendiente");

            entity.Property(e => e.motivo_rechazo)
               .HasColumnName("motivo_rechazo")
                .IsRequired(false); 

            entity.Property(e => e.created_at)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.updated_at)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });


        modelBuilder.Entity<Comision>(entity =>
        {
            // Configuración de la tabla con esquema
            entity.ToTable("comisiones", "plataforma");

            // Clave primaria
            entity.HasKey(c => c.Id);

            // Propiedades
            entity.Property(c => c.Id)
                  .HasColumnName("Id")
                  .ValueGeneratedOnAdd(); // Auto-incremental

            entity.Property(c => c.AgenciaId)
                  .HasColumnName("AgenciaId")
                  .IsRequired();

            entity.Property(c => c.Monto)
                  .HasColumnName("Monto")
                  .HasColumnType("numeric(18,2)")
                  .IsRequired();

            entity.Property(c => c.Concepto)
                  .HasColumnName("Concepto")
                  .HasColumnType("text");

            entity.Property(c => c.Referencia)
                  .HasColumnName("Referencia")
                  .HasColumnType("text");

            entity.Property(c => c.Fecha)
                  .HasColumnName("Fecha")
                  .HasColumnType("timestamp without time zone")
                  .IsRequired();

            entity.Property(c => c.CreatedAt)
                  .HasColumnName("CreatedAt")
                  .HasColumnType("timestamp without time zone")
                  .IsRequired();

            // Relación con Agencia
            entity.HasOne(c => c.Agencia)
                  .WithMany() // Si Agencia no tiene colección de Comisiones, déjalo vacío
                  .HasForeignKey(c => c.AgenciaId)
                  .HasConstraintName("FK_Comisiones_Agencias_AgenciaId")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<anuncios_destacado>(entity =>
        {
            entity.HasKey(e => e.id).HasName("anuncios_destacados_pkey");

            entity.ToTable("anuncios_destacados", "plataforma");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.esta_activo).HasDefaultValue(true);
            entity.Property(e => e.monto_pagado).HasPrecision(10, 2);
            entity.Property(e => e.tipo).HasMaxLength(50);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.payment_reference).HasMaxLength(100);

            entity.HasOne(d => d.acompanante).WithMany(p => p.anuncios_destacados)
                .HasForeignKey(d => d.acompanante_id)
                .HasConstraintName("anuncios_destacados_acompanante_id_fkey");

            entity.HasOne(d => d.cupon).WithMany(p => p.anuncios_destacados)
                .HasForeignKey(d => d.cupon_id)
                .HasConstraintName("anuncios_destacados_cupon_id_fkey");
        });

        modelBuilder.Entity<auth_proveedore>(entity =>
        {
            entity.HasKey(e => e.id).HasName("auth_proveedores_pkey");

            entity.ToTable("auth_proveedores", "plataforma", tb => tb.HasComment("Proveedores de autenticación soportados (Google, Facebook, etc)"));

            entity.HasIndex(e => e.nombre, "auth_proveedores_nombre_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.esta_activo).HasDefaultValue(true);
            entity.Property(e => e.nombre).HasMaxLength(50);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<categoria>(entity =>
        {
            entity.HasKey(e => e.id).HasName("categorias_pkey");

            entity.ToTable("categorias", "plataforma");

            entity.HasIndex(e => e.nombre, "categorias_nombre_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.nombre).HasMaxLength(100);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<cliente>(entity =>
        {
            entity.HasKey(e => e.id).HasName("clientes_pkey");

            entity.ToTable("clientes", "plataforma");

            entity.HasIndex(e => e.usuario_id, "clientes_usuario_id_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.es_vip).HasDefaultValue(false);
            entity.Property(e => e.nickname).HasMaxLength(100);
            entity.Property(e => e.puntos_acumulados).HasDefaultValue(0);
            entity.Property(e => e.stripe_customer_id).HasMaxLength(100);

            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.usuario).WithOne(p => p.cliente)
                .HasForeignKey<cliente>(d => d.usuario_id)
                .HasConstraintName("clientes_usuario_id_fkey");
        });

        modelBuilder.Entity<compras_paquete>(entity =>
        {
            entity.HasKey(e => e.id).HasName("compras_paquetes_pkey");

            entity.ToTable("compras_paquetes", "plataforma");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.estado)
                .HasMaxLength(50)
                .HasDefaultValueSql("'completado'::character varying");
            entity.Property(e => e.fecha_compra).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.metodo_pago).HasMaxLength(50);
            entity.Property(e => e.monto_pagado).HasPrecision(10, 2);
            entity.Property(e => e.referencia_pago).HasMaxLength(255);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.cliente).WithMany(p => p.compras_paquetes)
                .HasForeignKey(d => d.cliente_id)
                .HasConstraintName("compras_paquetes_cliente_id_fkey");

            entity.HasOne(d => d.paquete).WithMany(p => p.compras_paquetes)
                .HasForeignKey(d => d.paquete_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("compras_paquetes_paquete_id_fkey");
        });

        modelBuilder.Entity<contacto>(entity =>
        {
            entity.HasKey(e => e.id).HasName("contactos_pkey");

            entity.ToTable("contactos", "plataforma");

            entity.HasIndex(e => e.fecha_contacto, "idx_contactos_fecha");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.fecha_contacto).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ip_contacto).HasMaxLength(50);
            entity.Property(e => e.tipo_contacto).HasMaxLength(50);

            entity.HasOne(d => d.acompanante).WithMany(p => p.contactos)
                .HasForeignKey(d => d.acompanante_id)
                .HasConstraintName("contactos_acompanante_id_fkey");

            entity.HasOne(d => d.cliente).WithMany(p => p.contactos)
                .HasForeignKey(d => d.cliente_id)
                .HasConstraintName("contactos_cliente_id_fkey");
        });

        modelBuilder.Entity<cupones_cliente>(entity =>
        {
            entity.HasKey(e => e.id).HasName("cupones_cliente_pkey");

            entity.ToTable("cupones_cliente", "plataforma");

            entity.HasIndex(e => e.codigo, "cupones_cliente_codigo_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.codigo).HasMaxLength(50);
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.esta_usado).HasDefaultValue(false);
            entity.Property(e => e.fecha_creacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.cliente).WithMany(p => p.cupones_clientes)
                .HasForeignKey(d => d.cliente_id)
                .HasConstraintName("cupones_cliente_cliente_id_fkey");

            entity.HasOne(d => d.tipo_cupon).WithMany(p => p.cupones_clientes)
                .HasForeignKey(d => d.tipo_cupon_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("cupones_cliente_tipo_cupon_id_fkey");
        });

        // En tu OnModelCreating o clase de configuración:
        modelBuilder.Entity<foto>(entity =>
        {
            entity.HasKey(e => e.id).HasName("fotos_pkey");
            entity.ToTable("fotos", "plataforma");

            entity.Property(e => e.id)
                .UseIdentityAlwaysColumn();

            entity.Property(e => e.url)
                .HasMaxLength(512);

            entity.Property(e => e.es_principal)
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(e => e.orden)
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.verificada)                        // ← nueva columna
                .HasDefaultValue(true)
                .IsRequired()
                .HasColumnName("verificada");

            entity.Property(e => e.fecha_verificacion)                // ← nueva columna
                .HasDefaultValueSql("NOW()")
                .IsRequired()
                .HasColumnName("fecha_verificacion");

            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.HasOne(d => d.acompanante)
                .WithMany(p => p.fotos)
                .HasForeignKey(d => d.acompanante_id)
                .HasConstraintName("fotos_acompanante_id_fkey");
        });


        modelBuilder.Entity<intentos_login>(entity =>
        {
            entity.HasKey(e => e.id).HasName("intentos_login_pkey");

            entity.ToTable("intentos_login", "plataforma");

            entity.HasIndex(e => new { e.email, e.ip_address }, "idx_intentos_login_email_ip");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.intentos).HasDefaultValue(1);
            entity.Property(e => e.ip_address).HasMaxLength(50);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<membresias_vip>(entity =>
        {
            entity.HasKey(e => e.id).HasName("membresias_vip_pkey");

            entity.ToTable("membresias_vip", "plataforma");

            entity.HasIndex(e => e.nombre, "membresias_vip_nombre_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.descuento_anuncios).HasDefaultValue(0);
            entity.Property(e => e.nombre).HasMaxLength(100);
            entity.Property(e => e.precio_mensual).HasPrecision(10, 2);
            entity.Property(e => e.puntos_mensuales).HasDefaultValue(0);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.esta_activa).HasDefaultValue(true);

        });

        modelBuilder.Entity<movimientos_punto>(entity =>
        {
            entity.HasKey(e => e.id).HasName("movimientos_puntos_pkey");

            entity.ToTable("movimientos_puntos", "plataforma");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.concepto).HasMaxLength(255);
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.fecha).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.tipo).HasMaxLength(50);

            entity.HasOne(d => d.cliente).WithMany(p => p.movimientos_puntos)
                .HasForeignKey(d => d.cliente_id)
                .HasConstraintName("movimientos_puntos_cliente_id_fkey");
        });

        modelBuilder.Entity<paquete_cupones_detalle>(entity =>
        {
            entity.HasKey(e => e.id).HasName("paquete_cupones_detalle_pkey");

            entity.ToTable("paquete_cupones_detalle", "plataforma");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.cantidad).HasDefaultValue(1);
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.paquete).WithMany(p => p.paquete_cupones_detalles)
                .HasForeignKey(d => d.paquete_id)
                .HasConstraintName("paquete_cupones_detalle_paquete_id_fkey");

            entity.HasOne(d => d.tipo_cupon).WithMany(p => p.paquete_cupones_detalles)
                .HasForeignKey(d => d.tipo_cupon_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("paquete_cupones_detalle_tipo_cupon_id_fkey");
        });

        modelBuilder.Entity<paquetes_cupone>(entity =>
        {
            entity.HasKey(e => e.id).HasName("paquetes_cupones_pkey");

            entity.ToTable("paquetes_cupones", "plataforma");

            entity.HasIndex(e => e.nombre, "paquetes_cupones_nombre_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.activo).HasDefaultValue(true);
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.incluye_sorteo).HasDefaultValue(false);
            entity.Property(e => e.nombre).HasMaxLength(100);
            entity.Property(e => e.precio).HasPrecision(10, 2);
            entity.Property(e => e.puntos_otorgados).HasDefaultValue(0);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<participantes_sorteo>(entity =>
        {
            entity.HasKey(e => e.id).HasName("participantes_sorteo_pkey");

            entity.ToTable("participantes_sorteo", "plataforma");

            entity.HasIndex(e => new { e.sorteo_id, e.cliente_id }, "participantes_sorteo_sorteo_id_cliente_id_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.es_ganador).HasDefaultValue(false);
            entity.Property(e => e.fecha_participacion).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.cliente).WithMany(p => p.participantes_sorteos)
                .HasForeignKey(d => d.cliente_id)
                .HasConstraintName("participantes_sorteo_cliente_id_fkey");

            entity.HasOne(d => d.sorteo).WithMany(p => p.participantes_sorteos)
                .HasForeignKey(d => d.sorteo_id)
                .HasConstraintName("participantes_sorteo_sorteo_id_fkey");
        });

        modelBuilder.Entity<refresh_token>(entity =>
        {
            entity.HasKey(e => e.id).HasName("refresh_tokens_pkey");

            entity.ToTable("refresh_tokens", "plataforma");

            entity.HasIndex(e => e.token, "idx_refresh_tokens_token");

            entity.HasIndex(e => e.token, "refresh_tokens_token_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.device_info).HasMaxLength(255);
            entity.Property(e => e.esta_revocado).HasDefaultValue(false);
            entity.Property(e => e.ip_address).HasMaxLength(50);
            entity.Property(e => e.token).HasMaxLength(255);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.usuario).WithMany(p => p.refresh_tokens)
                .HasForeignKey(d => d.usuario_id)
                .HasConstraintName("refresh_tokens_usuario_id_fkey");
        });

        modelBuilder.Entity<role>(entity =>
        {
            entity.HasKey(e => e.id).HasName("roles_pkey");

            entity.ToTable("roles", "plataforma");

            entity.HasIndex(e => e.nombre, "roles_nombre_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.nombre).HasMaxLength(50);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<servicio>(entity =>
        {
            entity.HasKey(e => e.id).HasName("servicios_pkey");

            entity.ToTable("servicios", "plataforma");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.nombre).HasMaxLength(100);
            entity.Property(e => e.precio).HasPrecision(10, 2);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.acompanante).WithMany(p => p.servicios)
                .HasForeignKey(d => d.acompanante_id)
                .HasConstraintName("servicios_acompanante_id_fkey");
        });

        modelBuilder.Entity<sorteo>(entity =>
        {
            entity.HasKey(e => e.id).HasName("sorteos_pkey");

            entity.ToTable("sorteos", "plataforma");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.esta_activo).HasDefaultValue(true);
            entity.Property(e => e.nombre).HasMaxLength(255);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<suscripciones_vip>(entity =>
        {
            entity.HasKey(e => e.id).HasName("suscripciones_vip_pkey");

            entity.ToTable("suscripciones_vip", "plataforma");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.es_renovacion_automatica).HasDefaultValue(true);
            entity.Property(e => e.estado)
                .HasMaxLength(50)
                .HasDefaultValueSql("'activa'::character varying");
            entity.Property(e => e.fecha_inicio).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.metodo_pago).HasMaxLength(50);
            entity.Property(e => e.referencia_pago).HasMaxLength(255);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.cliente).WithMany(p => p.suscripciones_vips)
                .HasForeignKey(d => d.cliente_id)
                .HasConstraintName("suscripciones_vip_cliente_id_fkey");

            entity.HasOne(d => d.membresia).WithMany(p => p.suscripciones_vips)
                .HasForeignKey(d => d.membresia_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("suscripciones_vip_membresia_id_fkey");
        });

        modelBuilder.Entity<tipos_cupone>(entity =>
        {
            entity.HasKey(e => e.id).HasName("tipos_cupones_pkey");

            entity.ToTable("tipos_cupones", "plataforma");

            entity.HasIndex(e => e.nombre, "tipos_cupones_nombre_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.nombre).HasMaxLength(100);
            entity.Property(e => e.porcentaje_descuento).HasPrecision(5, 2);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<tokens_reset_password>(entity =>
        {
            entity.HasKey(e => e.id).HasName("tokens_reset_password_pkey");

            entity.ToTable("tokens_reset_password", "plataforma");

            entity.HasIndex(e => e.token, "idx_reset_password_token");

            entity.HasIndex(e => e.token, "tokens_reset_password_token_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.esta_usado).HasDefaultValue(false);
            entity.Property(e => e.token).HasMaxLength(100);

            entity.HasOne(d => d.usuario).WithMany(p => p.tokens_reset_passwords)
                .HasForeignKey(d => d.usuario_id)
                .HasConstraintName("tokens_reset_password_usuario_id_fkey");
        });

        modelBuilder.Entity<usuario>(entity =>
        {
            entity.HasKey(e => e.id).HasName("usuarios_pkey");

            entity.ToTable("usuarios", "plataforma");

            entity.HasIndex(e => e.email, "usuarios_email_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.esta_activo).HasDefaultValue(true);
            entity.Property(e => e.fecha_registro).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.password_hash).HasMaxLength(255);
            entity.Property(e => e.password_required).HasDefaultValue(true);
            entity.Property(e => e.provider)
                .HasMaxLength(50)
                .HasDefaultValueSql("'local'::character varying");
            entity.Property(e => e.telefono).HasMaxLength(20);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.rol).WithMany(p => p.usuarios)
                .HasForeignKey(d => d.rol_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usuarios_rol_id_fkey");
        });

        modelBuilder.Entity<usuario_auth_externo>(entity =>
        {
            entity.HasKey(e => e.id).HasName("usuario_auth_externo_pkey");

            entity.ToTable("usuario_auth_externo", "plataforma", tb => tb.HasComment("Asociación entre usuarios y sus cuentas en proveedores externos"));

            entity.HasIndex(e => new { e.proveedor_id, e.proveedor_user_id }, "idx_usuario_auth_externo_ids");

            entity.HasIndex(e => new { e.proveedor_id, e.proveedor_user_id }, "usuario_auth_externo_proveedor_id_proveedor_user_id_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.datos_extra).HasColumnType("jsonb");
            entity.Property(e => e.email_verificado).HasDefaultValue(false);
            entity.Property(e => e.proveedor_user_id).HasMaxLength(255);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.proveedor).WithMany(p => p.usuario_auth_externos)
                .HasForeignKey(d => d.proveedor_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usuario_auth_externo_proveedor_id_fkey");

            entity.HasOne(d => d.usuario).WithMany(p => p.usuario_auth_externos)
                .HasForeignKey(d => d.usuario_id)
                .HasConstraintName("usuario_auth_externo_usuario_id_fkey");
        });

        modelBuilder.Entity<verificacione>(entity =>
        {
            entity.HasKey(e => e.id).HasName("verificaciones_pkey");

            entity.ToTable("verificaciones", "plataforma");

            entity.HasIndex(e => e.acompanante_id, "verificaciones_acompanante_id_key").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.estado)
                .HasMaxLength(50)
                .HasDefaultValueSql("'aprobada'::character varying");
            entity.Property(e => e.fecha_verificacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.monto_cobrado).HasPrecision(10, 2);
            entity.Property(e => e.updated_at).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.acompanante).WithOne(p => p.verificacione)
                .HasForeignKey<verificacione>(d => d.acompanante_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("verificaciones_acompanante_id_fkey");

            entity.HasOne(d => d.agencia).WithMany(p => p.verificaciones)
                .HasForeignKey(d => d.agencia_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("verificaciones_agencia_id_fkey");
        });

        modelBuilder.Entity<visitas_perfil>(entity =>
        {
            entity.HasKey(e => e.id).HasName("visitas_perfil_pkey");

            entity.ToTable("visitas_perfil", "plataforma");

            entity.HasIndex(e => e.fecha_visita, "idx_visitas_perfil_fecha");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.created_at).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.fecha_visita).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ip_visitante).HasMaxLength(50);

            entity.HasOne(d => d.acompanante).WithMany(p => p.visitas_perfils)
                .HasForeignKey(d => d.acompanante_id)
                .HasConstraintName("visitas_perfil_acompanante_id_fkey");

            entity.HasOne(d => d.cliente).WithMany(p => p.visitas_perfils)
                .HasForeignKey(d => d.cliente_id)
                .HasConstraintName("visitas_perfil_cliente_id_fkey");
        });

        modelBuilder.Entity<vw_agencias_acompanante>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_agencias_acompanantes", "plataforma");

            entity.Property(e => e.agencia_nombre).HasMaxLength(255);
        });

        modelBuilder.Entity<vw_clientes_info>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_clientes_info", "plataforma");

            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.nickname).HasMaxLength(100);
        });

        modelBuilder.Entity<vw_perfiles_destacado>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_perfiles_destacados", "plataforma");

            entity.Property(e => e.ciudad).HasMaxLength(100);
            entity.Property(e => e.genero).HasMaxLength(50);
            entity.Property(e => e.idiomas).HasMaxLength(255);
            entity.Property(e => e.moneda).HasMaxLength(10);
            entity.Property(e => e.nombre_perfil).HasMaxLength(100);
            entity.Property(e => e.pais).HasMaxLength(100);
            entity.Property(e => e.tarifa_base).HasPrecision(10, 2);
            entity.Property(e => e.tipo_destacado).HasMaxLength(50);
        });

        modelBuilder.Entity<vw_perfiles_populare>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_perfiles_populares", "plataforma");

            entity.Property(e => e.ciudad).HasMaxLength(100);
            entity.Property(e => e.moneda).HasMaxLength(10);
            entity.Property(e => e.nombre_perfil).HasMaxLength(100);
            entity.Property(e => e.tarifa_base).HasPrecision(10, 2);
        });

        modelBuilder.Entity<vw_perfiles_reciente>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_perfiles_recientes", "plataforma");

            entity.Property(e => e.ciudad).HasMaxLength(100);
            entity.Property(e => e.foto_principal).HasMaxLength(512);
            entity.Property(e => e.genero).HasMaxLength(50);
            entity.Property(e => e.idiomas).HasMaxLength(255);
            entity.Property(e => e.moneda).HasMaxLength(10);
            entity.Property(e => e.nombre_perfil).HasMaxLength(100);
            entity.Property(e => e.pais).HasMaxLength(100);
            entity.Property(e => e.tarifa_base).HasPrecision(10, 2);
        });

        modelBuilder.Entity<vw_ranking_perfile>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ranking_perfiles", "plataforma");

            entity.Property(e => e.ciudad).HasMaxLength(100);
            entity.Property(e => e.nombre_perfil).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
