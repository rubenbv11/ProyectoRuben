using ProyectoRuben.Backen.Modelo;

namespace ProyectoRuben.Backend.Servicios
{
	/// <summary>
	/// Singleton estático que mantiene en memoria el usuario autenticado
	/// durante toda la sesión de la aplicación.
	/// Todos los ViewModels pueden consultarlo sin inyección de dependencias.
	/// </summary>
	public static class SesionUsuario
	{

		public static Usuario? UsuarioActual { get; private set; }

		public static bool HaySesion => UsuarioActual != null;

		public static bool EsAdministrador =>
			UsuarioActual?.Rol == "Administrador";

		public static void IniciarSesion(Usuario usuario)
		{
			UsuarioActual = usuario;
		}
		public static void CerrarSesion()
		{
			UsuarioActual = null;
		}
	}
}