using AgencyPlatform.Application.Interfaces.Services;
using AgencyPlatform.Application.Interfaces.Services.EmailAgencia;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Infrastructure.Services.EmailProfecional
{
    public class EmailProfesionalService : IEmailProfesionalService
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<EmailProfesionalService> _logger;

        public EmailProfesionalService(IEmailSender emailSender, ILogger<EmailProfesionalService> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        #region Agencia Emails

        public async Task EnviarCorreoAprobacionAgencia(string email, string nombreAgencia)
        {
            var asunto = "¡Felicidades! Tu agencia ha sido aprobada";
            var mensaje = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }}
                    .header {{ background-color: #4a90e2; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .footer {{ background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; }}
                    .button {{ background-color: #4CAF50; color: white; padding: 10px 15px; text-decoration: none; border-radius: 4px; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>¡Tu agencia ha sido aprobada!</h1>
                </div>
                <div class='content'>
                    <p>Estimado administrador de <strong>{nombreAgencia}</strong>,</p>
                    
                    <p>Nos complace informarte que tu agencia ha sido <strong>verificada y aprobada</strong> en nuestra plataforma.</p>
                    
                    <p>A partir de ahora podrás:</p>
                    <ul>
                        <li>Verificar perfiles de acompañantes</li>
                        <li>Crear anuncios destacados</li>
                        <li>Acceder a estadísticas detalladas</li>
                        <li>Recibir comisiones y acumular puntos</li>
                    </ul>
                    
                    <p>Te invitamos a acceder a tu panel de control y comenzar a gestionar tu agencia:</p>
                    <p style='text-align: center;'>
                        <a href='https://tudominio.com/dashboard' class='button'>Acceder al panel</a>
                    </p>
                    
                    <p>Si tienes alguna pregunta o necesitas asistencia, no dudes en contactar a nuestro equipo de soporte.</p>
                    
                    <p>¡Bienvenido a nuestra red de agencias verificadas!</p>
                </div>
                <div class='footer'>
                    <p>Este mensaje fue enviado desde una dirección de correo no monitoreada. Por favor no respondas a este correo.</p>
                    <p>&copy; 2025 Tu Plataforma - Todos los derechos reservados</p>
                </div>
            </body>
            </html>
            ";

            try
            {
                await _emailSender.SendEmailAsync(email, asunto, mensaje);
                _logger.LogInformation($"Correo de aprobación enviado a agencia {nombreAgencia} <{email}>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo de aprobación a agencia {nombreAgencia} <{email}>");
            }
        }


        public async Task EnviarCorreoRechazoAgencia(string email, string nombreAgencia, string motivo)
        {
            var asunto = "Respuesta a tu solicitud de registro de agencia";
            var mensaje = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }}
                    .header {{ background-color: #f44336; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .footer {{ background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; }}
                    .reason {{ background-color: #fff8e1; border-left: 4px solid #ffa000; padding: 10px; margin: 15px 0; }}
                    .button {{ background-color: #4CAF50; color: white; padding: 10px 15px; text-decoration: none; border-radius: 4px; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>Solicitud de Agencia Revisada</h1>
                </div>
                <div class='content'>
                    <p>Estimado administrador de <strong>{nombreAgencia}</strong>,</p>
                    
                    <p>Hemos revisado tu solicitud para registrar tu agencia en nuestra plataforma y lamentamos informarte que no ha sido aprobada en esta ocasión.</p>
                    
                    <div class='reason'>
                        <p><strong>Motivo:</strong> {motivo}</p>
                    </div>
                    
                    <p>Puedes enviar una nueva solicitud abordando los puntos mencionados anteriormente. Nuestro equipo estará encantado de reconsiderar tu solicitud una vez que se hayan realizado los ajustes necesarios.</p>
                    
                    <p>Si tienes alguna pregunta o necesitas aclaraciones, no dudes en contactar a nuestro equipo de soporte.</p>
                    
                    <p>Esperamos poder colaborar contigo pronto.</p>
                </div>
                <div class='footer'>
                    <p>Este mensaje fue enviado desde una dirección de correo no monitoreada. Por favor no respondas a este correo.</p>
                    <p>&copy; 2025 Tu Plataforma - Todos los derechos reservados</p>
                </div>
            </body>
            </html>
            ";

            try
            {
                await _emailSender.SendEmailAsync(email, asunto, mensaje);
                _logger.LogInformation($"Correo de rechazo enviado a agencia {nombreAgencia} <{email}>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo de rechazo a agencia {nombreAgencia} <{email}>");
            }
        }

        public async Task EnviarCorreoVerificacionAgencia(string email, string nombreAgencia)
        {
            var asunto = "¡Tu agencia ha sido verificada!";
            var mensaje = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }}
                    .header {{ background-color: #4a90e2; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .footer {{ background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; }}
                    .badge {{ display: inline-block; background-color: #28a745; color: white; padding: 5px 10px; border-radius: 20px; margin-bottom: 15px; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>¡Agencia Verificada!</h1>
                </div>
                <div class='content'>
                    <p>Estimado administrador de <strong>{nombreAgencia}</strong>,</p>
                    
                    <div style='text-align: center;'>
                        <span class='badge'>✓ VERIFICADA</span>
                    </div>
                    
                    <p>Nos complace informarte que tu agencia ha sido <strong>verificada oficialmente</strong> en nuestra plataforma.</p>
                    
                    <p>¿Qué significa esto?</p>
                    <ul>
                        <li>Tu agencia mostrará un distintivo de verificación</li>
                        <li>Podrás verificar a tus acompañantes</li>
                        <li>Generarás mayor confianza entre los usuarios</li>
                        <li>Tendrás acceso a funcionalidades exclusivas</li>
                        <li>Comenzarás a recibir comisiones por verificaciones (5% inicial)</li>
                    </ul>
                    
                    <p>Te recomendamos comenzar a verificar a tus acompañantes para potenciar la visibilidad de tu agencia.</p>
                </div>
                <div class='footer'>
                    <p>Este mensaje fue enviado desde una dirección de correo no monitoreada. Por favor no respondas a este correo.</p>
                    <p>&copy; 2025 Tu Plataforma - Todos los derechos reservados</p>
                </div>
            </body>
            </html>
            ";

            try
            {
                await _emailSender.SendEmailAsync(email, asunto, mensaje);
                _logger.LogInformation($"Correo de verificación enviado a agencia {nombreAgencia} <{email}>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo de verificación a agencia {nombreAgencia} <{email}>");
            }
        }

        #endregion
        #region Acompañante Emails

        public async Task EnviarCorreoVerificacionAcompanante(string email, string nombreAcompanante, string nombreAgencia)
        {
            var asunto = "¡Tu perfil ha sido verificado!";
            var mensaje = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }}
                    .header {{ background-color: #9932CC; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .footer {{ background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; }}
                    .badge {{ display: inline-block; background-color: #28a745; color: white; padding: 5px 10px; border-radius: 20px; margin-bottom: 15px; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>¡Perfil Verificado!</h1>
                </div>
                <div class='content'>
                    <p>Hola <strong>{nombreAcompanante}</strong>,</p>
                    
                    <div style='text-align: center;'>
                        <span class='badge'>✓ VERIFICADO</span>
                    </div>
                    
                    <p>Nos complace informarte que tu perfil ha sido <strong>verificado oficialmente</strong> por <strong>{nombreAgencia}</strong>.</p>
                    
                    <p>¿Qué significa esto?</p>
                    <ul>
                        <li>Tu perfil mostrará un distintivo de verificación</li>
                        <li>Aparecerás con prioridad en las búsquedas</li>
                        <li>Generarás mayor confianza entre los usuarios</li>
                        <li>Tendrás acceso a estadísticas avanzadas</li>
                    </ul>
                    
                    <p>Te invitamos a revisar tu perfil con el nuevo estado de verificación.</p>
                    
                    <p>Recuerda mantener tu información actualizada para potenciar tu visibilidad en la plataforma.</p>
                </div>
                <div class='footer'>
                    <p>Este mensaje fue enviado desde una dirección de correo no monitoreada. Por favor no respondas a este correo.</p>
                    <p>&copy; 2025 Tu Plataforma - Todos los derechos reservados</p>
                </div>
            </body>
            </html>
            ";

            try
            {
                await _emailSender.SendEmailAsync(email, asunto, mensaje);
                _logger.LogInformation($"Correo de verificación enviado a acompañante {nombreAcompanante} <{email}>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo de verificación a acompañante {nombreAcompanante} <{email}>");
            }
        }

        public async Task EnviarCorreoSolicitudAprobada(string email, string nombreAcompanante, string nombreAgencia)
        {
            var asunto = "¡Tu solicitud a la agencia ha sido aprobada!";
            var mensaje = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .footer {{ background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; }}
                    .success {{ background-color: #e8f5e9; border-left: 4px solid #4CAF50; padding: 10px; margin: 15px 0; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>¡Solicitud Aprobada!</h1>
                </div>
                <div class='content'>
                    <p>Hola <strong>{nombreAcompanante}</strong>,</p>
                    
                    <div class='success'>
                        <p>¡Buenas noticias! Tu solicitud para unirte a <strong>{nombreAgencia}</strong> ha sido aprobada.</p>
                    </div>
                    
                    <p>A partir de ahora, perteneces a <strong>{nombreAgencia}</strong> y podrás disfrutar de los siguientes beneficios:</p>
                    
                    <ul>
                        <li>Mayor visibilidad en la plataforma</li>
                        <li>Posibilidad de obtener verificación oficial</li>
                        <li>Mejor posicionamiento en búsquedas</li>
                        <li>Acceso a estadísticas detalladas</li>
                    </ul>
                    
                    <p>Te recomendamos mantener tu perfil actualizado y consultar con <strong>{nombreAgencia}</strong> las opciones de verificación disponibles.</p>
                    
                    <p>¡Te deseamos mucho éxito con tu nueva agencia!</p>
                </div>
                <div class='footer'>
                    <p>Este mensaje fue enviado desde una dirección de correo no monitoreada. Por favor no respondas a este correo.</p>
                    <p>&copy; 2025 Tu Plataforma - Todos los derechos reservados</p>
                </div>
            </body>
            </html>
            ";

            try
            {
                await _emailSender.SendEmailAsync(email, asunto, mensaje);
                _logger.LogInformation($"Correo de solicitud aprobada enviado a acompañante {nombreAcompanante} <{email}>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo de solicitud aprobada a acompañante {nombreAcompanante} <{email}>");
            }
        }

        public async Task EnviarCorreoSolicitudRechazada(string email, string nombreAcompanante, string nombreAgencia, string motivo)
        {
            var asunto = "Respuesta a tu solicitud de agencia";
            var mensaje = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }}
                    .header {{ background-color: #f44336; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .footer {{ background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; }}
                    .reason {{ background-color: #fff8e1; border-left: 4px solid #ffa000; padding: 10px; margin: 15px 0; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>Respuesta a tu Solicitud</h1>
                </div>
                <div class='content'>
                    <p>Hola <strong>{nombreAcompanante}</strong>,</p>
                    
                    <p>Te informamos que tu solicitud para unirte a <strong>{nombreAgencia}</strong> no ha sido aprobada en esta ocasión.</p>
                    
                    <div class='reason'>
                        <p><strong>Motivo:</strong> {motivo}</p>
                    </div>
                    
                    <p>No te preocupes, aún puedes:</p>
                    <ul>
                        <li>Enviar una solicitud a otra agencia</li>
                        <li>Continuar como perfil independiente</li>
                        <li>Mejorar tu perfil y solicitar de nuevo en el futuro</li>
                    </ul>
                    
                    <p>Si tienes alguna pregunta, no dudes en contactar a nuestro equipo de soporte.</p>
                    
                    <p>¡Te deseamos mucho éxito en la plataforma!</p>
                </div>
                <div class='footer'>
                    <p>Este mensaje fue enviado desde una dirección de correo no monitoreada. Por favor no respondas a este correo.</p>
                    <p>&copy; 2025 Tu Plataforma - Todos los derechos reservados</p>
                </div>
            </body>
            </html>
            ";

            try
            {
                await _emailSender.SendEmailAsync(email, asunto, mensaje);
                _logger.LogInformation($"Correo de solicitud rechazada enviado a acompañante {nombreAcompanante} <{email}>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo de solicitud rechazada a acompañante {nombreAcompanante} <{email}>");
            }
        }

        public async Task EnviarCorreoInvitacionAgencia(string email, string nombreAcompanante, string nombreAgencia)
        {
            var asunto = $"Invitación de {nombreAgencia} para unirte a su equipo";
            var mensaje = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }}
                    .header {{ background-color: #FF6347; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .footer {{ background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; }}
                    .invitation {{ background-color: #f0f4ff; border-left: 4px solid #4a90e2; padding: 10px; margin: 15px 0; }}
                    .button {{ display: inline-block; background-color: #4CAF50; color: white; padding: 10px 15px; text-decoration: none; border-radius: 4px; margin: 5px; }}
                    .button.reject {{ background-color: #f44336; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>Invitación Especial</h1>
                </div>
                <div class='content'>
                    <p>Hola <strong>{nombreAcompanante}</strong>,</p>
                    
                    <div class='invitation'>
                        <p>La agencia <strong>{nombreAgencia}</strong> te ha invitado a formar parte de su equipo.</p>
                    </div>
                    
                    <p>Esta es una gran oportunidad que te permitirá:</p>
                    <ul>
                        <li>Obtener mayor visibilidad en la plataforma</li>
                        <li>Acceder a la verificación oficial de perfil</li>
                        <li>Mejorar tu posicionamiento en búsquedas</li>
                        <li>Formar parte de una agencia reconocida</li>
                    </ul>
                    
                    <p>Para responder a esta invitación, inicia sesión en tu cuenta y accede a la sección 'Invitaciones Pendientes'.</p>
                    
                    <p style='text-align: center;'>
                        <a href='https://tudominio.com/invitaciones' class='button'>Ver Invitación</a>
                    </p>
                    
                    <p>La invitación permanecerá activa por 7 días. Si tienes preguntas, no dudes en contactar a nuestro equipo de soporte.</p>
                </div>
                <div class='footer'>
                    <p>Este mensaje fue enviado desde una dirección de correo no monitoreada. Por favor no respondas a este correo.</p>
                    <p>&copy; 2025 Tu Plataforma - Todos los derechos reservados</p>
                </div>
            </body>
            </html>
            ";

            try
            {
                await _emailSender.SendEmailAsync(email, asunto, mensaje);
                _logger.LogInformation($"Correo de invitación enviado a acompañante {nombreAcompanante} <{email}>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo de invitación a acompañante {nombreAcompanante} <{email}>");
            }
        }

        #endregion

        #region Pagos Emails

        public async Task EnviarConfirmacionPago(string email, string nombreUsuario, string conceptoPago, decimal monto, string referencia)
        {
            var asunto = "Confirmación de pago exitoso";
            var fecha = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            var mensaje = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }}
                    .header {{ background-color: #20B2AA; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .footer {{ background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; }}
                    .detail {{ background-color: #f9f9f9; border-left: 4px solid #20B2AA; padding: 10px; margin: 15px 0; }}
                    table {{ width: 100%; border-collapse: collapse; }}
                    th, td {{ padding: 8px; text-align: left; border-bottom: 1px solid #ddd; }}
                    th {{ background-color: #f2f2f2; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>Pago Confirmado</h1>
                </div>
                <div class='content'>
                    <p>Hola <strong>{nombreUsuario}</strong>,</p>
                    
                    <p>Hemos recibido tu pago correctamente. A continuación, te presentamos los detalles de la transacción:</p>
                    
                    <div class='detail'>
                        <table>
                            <tr>
                                <th>Concepto:</th>
                                <td>{conceptoPago}</td>
                            </tr>
                            <tr>
                                <th>Monto:</th>
                                <td>${monto.ToString("0.00")}</td>
                            </tr>
                            <tr>
                                <th>Fecha:</th>
                                <td>{fecha}</td>
                            </tr>
                            <tr>
                                <th>Referencia:</th>
                                <td>{referencia}</td>
                            </tr>
                            <tr>
                                <th>Estado:</th>
                                <td><strong style='color: green;'>COMPLETADO</strong></td>
                            </tr>
                        </table>
                    </div>
                    
                    <p>Este pago ha sido procesado y registrado en tu cuenta. Puedes ver el historial completo de tus transacciones en tu panel de control.</p>
                    
                    <p>Gracias por tu confianza.</p>
                </div>
                <div class='footer'>
                    <p>Este mensaje fue enviado desde una dirección de correo no monitoreada. Por favor no respondas a este correo.</p>
                    <p>&copy; 2025 Tu Plataforma - Todos los derechos reservados</p>
                </div>
            </body>
            </html>
            ";

            try
            {
                await _emailSender.SendEmailAsync(email, asunto, mensaje);
                _logger.LogInformation($"Correo de confirmación de pago enviado a {nombreUsuario} <{email}>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo de confirmación de pago a {nombreUsuario} <{email}>");
            }
        }

        #endregion

        #region Cupones y Puntos Emails

        public async Task EnviarConfirmacionCompraPaquete(string email, string nombreCliente, string nombrePaquete, decimal monto, int puntos, List<string> cupones)
        {
            var asunto = $"Tu compra de {nombrePaquete} ha sido confirmada";

            var mensaje = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }}
                    .header {{ background-color: #6A5ACD; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .footer {{ background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; }}
                    .item {{ background-color: #f9f9f9; border-left: 4px solid #6A5ACD; padding: 10px; margin: 10px 0; }}
                    .code {{ font-family: monospace; background-color: #f2f2f2; padding: 2px 5px; border-radius: 3px; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>¡Compra Exitosa!</h1>
                </div>
                <div class='content'>
                    <p>Hola <strong>{nombreCliente}</strong>,</p>
                    
                    <p>Tu compra del paquete <strong>{nombrePaquete}</strong> ha sido procesada con éxito.</p>
                    
                    <div class='item'>
                        <p><strong>Detalles de la compra:</strong></p>
                        <p>Paquete: {nombrePaquete}</p>
                        <p>Monto: ${monto.ToString("0.00")}</p>
                        <p>Puntos otorgados: +{puntos} puntos</p>
                        <p>Fecha: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}</p>
                    </div>
                    
                    <p><strong>Cupones adquiridos:</strong></p>
                    {string.Join("", cupones.Select(c => $"<div class='item'><p>Código: <span class='code'>{c}</span></p></div>"))}
                    
                    <p>Estos cupones ya están disponibles en tu cuenta y puedes utilizarlos en cualquier momento para destacar perfiles o comprar servicios premium.</p>
                    
                    <p>Recuerda que los puntos y cupones tienen una fecha de vencimiento. Consulta los términos y condiciones en nuestra plataforma.</p>
                    
                    <p>¡Gracias por tu compra!</p>
                </div>
                <div class='footer'>
                    <p>Este mensaje fue enviado desde una dirección de correo no monitoreada. Por favor no respondas a este correo.</p>
                    <p>&copy; 2025 Tu Plataforma - Todos los derechos reservados</p>
                </div>
            </body>
            </html>
            ";

            try
            {
                await _emailSender.SendEmailAsync(email, asunto, mensaje);
                _logger.LogInformation($"Correo de confirmación de compra de paquete enviado a {nombreCliente} <{email}>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo de confirmación de compra de paquete a {nombreCliente} <{email}>");
            }
        }

        #endregion

        #region Membresías VIP Emails

        public async Task EnviarConfirmacionSuscripcionVIP(string email, string nombreCliente, string nombreMembresia, decimal precio, string fechaRenovacion)
        {
            var asunto = "¡Bienvenido al exclusivo Club VIP!";

            var mensaje = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }}
                    .header {{ background: linear-gradient(to right, #DAA520, #B8860B); color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .footer {{ background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; }}
                    .benefit {{ background-color: #fff8e1; border-left: 4px solid #DAA520; padding: 10px; margin: 10px 0; }}
                    .vip-badge {{ display: inline-block; background-color: #DAA520; color: white; padding: 5px 15px; border-radius: 20px; font-weight: bold; margin: 10px 0; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>¡Bienvenido al Club VIP!</h1>
                </div>
                <div class='content'>
                    <p>Hola <strong>{nombreCliente}</strong>,</p>
                    
                    <p style='text-align: center;'><span class='vip-badge'>MIEMBRO VIP</span></p>
                    
                    <p>¡Felicidades por unirte a nuestro exclusivo Club VIP con la membresía <strong>{nombreMembresia}</strong>!</p>
                    
                    <p>Como miembro VIP, ahora disfrutas de estos beneficios exclusivos:</p>
                    
                    <div class='benefit'>
                        <p>✓ Menos anuncios en la plataforma</p>
                    </div>
                    <div class='benefit'>
                        <p>✓ Acumulación de puntos acelerada (20% extra)</p>
                    </div>
                    <div class='benefit'>
                        <p>✓ Descuentos exclusivos en servicios premium</p>
                    </div>
                    <div class='benefit'>
                        <p>✓ Participación prioritaria en sorteos especiales</p>
                    </div>
                    
                    <p><strong>Detalles de tu suscripción:</strong></p>
                    <ul>
                        <li>Membresía: {nombreMembresia}</li>
                        <li>Precio mensual: ${precio.ToString("0.00")}</li>
                        <li>Próxima renovación: {fechaRenovacion}</li>
                    </ul>
                    
                    <p>Tu suscripción se renovará automáticamente en la fecha indicada. Puedes cancelar la renovación automática en cualquier momento desde tu panel de control.</p>
                    
                    <p>¡Gracias por confiar en nosotros y disfruta de tu experiencia VIP!</p>
                </div>
                <div class='footer'>
                    <p>Este mensaje fue enviado desde una dirección de correo no monitoreada. Por favor no respondas a este correo.</p>
                    <p>&copy; 2025 Tu Plataforma - Todos los derechos reservados</p>
                </div>
            </body>
            </html>
            ";

            try
            {
                await _emailSender.SendEmailAsync(email, asunto, mensaje);
                _logger.LogInformation($"Correo de confirmación de suscripción VIP enviado a {nombreCliente} <{email}>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo de confirmación de suscripción VIP a {nombreCliente} <{email}>");
            }
        }

        #endregion

        #region Admin Emails

        public async Task EnviarNotificacionAdministrador(string email, string asunto, string mensaje)
        {
            var mensajeFormateado = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; }}
                    .header {{ background-color: #333; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; }}
                    .footer {{ background-color: #f2f2f2; padding: 10px; text-align: center; font-size: 12px; }}
                    .alert {{ background-color: #f8f8f8; border-left: 4px solid #333; padding: 10px; margin: 15px 0; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h1>Notificación Administrativa</h1>
                </div>
                <div class='content'>
                    <div class='alert'>
                        {mensaje}
                    </div>
                    
                    <p>Esta notificación ha sido generada automáticamente por el sistema.</p>
                </div>
                <div class='footer'>
                    <p>Este mensaje fue enviado desde una dirección de correo no monitoreada. Por favor no respondas a este correo.</p>
                    <p>&copy; 2025 Panel de Administración - Todos los derechos reservados</p>
                </div>
            </body>
            </html>
            ";

            try
            {
                await _emailSender.SendEmailAsync(email, asunto, mensajeFormateado);
                _logger.LogInformation($"Correo de notificación de administrador enviado a <{email}>");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar correo de notificación de administrador a <{email}>");
            }
        }

        #endregion



    }
}


