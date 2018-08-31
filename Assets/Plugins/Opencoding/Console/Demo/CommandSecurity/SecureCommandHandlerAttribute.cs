using System;

namespace Opencoding.Demo.CommandSecurity
{
	/// <summary>
	/// This attribute prevents users from being able to execute a command without entering a password.
	/// The ConsoleCommandSecurity behaviour must exist in the scene for this to work.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
	public class SecureCommandHandlerAttribute : Attribute
	{
		
	}
}