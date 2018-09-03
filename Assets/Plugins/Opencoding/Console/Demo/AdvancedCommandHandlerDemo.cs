using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Assets.Opencoding.Console.Demo
{
    // This class shows some advanced commands. By default, this isn't in the Demo scene. To experiment with
    // these commands, just add the component to a game object in the demo scene.
    class AdvancedCommandHandlerDemo : MonoBehaviour
    {
        private void Awake()
        {
            // This registers command handlers like normal, the only interesting thing here is that the only method 
            // in this class that has a [CommandHandler] attribute has a single parameter of "params string[]".
            CommandHandlers.RegisterCommandHandlers(typeof (AdvancedCommandHandlerDemo));

            // This method will register a command 'on the fly'. Optionally, you can specify the name and description.
            CommandHandlers.RegisterCommandHandler<string, int>(DynamicallyRegisteredMethod);

            // This method registers a command on the fly, but in this case, we're registering a method that has a params[] argument.
            // The user will be able to pass 0 or more strings into this command.
            CommandHandlers.RegisterCommandHandler<int, string[]>(DynamicParamsMethod1, "DynamicParamsMethod1");

            // This method allows you to specify the number of values that should be put into the 'params' parameter.
            // This only works in the params parameter is of type object[].
            // It'll apply the normal rules for commands - e.g. checking the right number of parameters, and values
            // passed to your function will be of the types you specify.
            CommandHandlers.RegisterCommandHandler<Color, string[]>(DynamicParamsMethod2, "DynamicParamsMethod2", 
                "Method that tests dynamic parameters with overridden types",
                 new[] { typeof(int), typeof(string) } );

            // This is similar to the above, except you can specify more information about the parameters
            // you're passing, e.g. names, default values etc.
            CommandHandlers.RegisterCommandHandler<Color, string[]>(DynamicParamsMethod2, "DynamicParamsMethod3",
                "Method that tests dynamic parameters with overridden full parameter infos",
                 new[]
                 {
                     new ParamInfo()
                     {
                         Type = typeof(int),
                         Name = "magicNumber"
                     },
                     new ParamInfo()
                     {
                         Type = typeof(string),
                         IsOptional = true, // if you set IsOptional, make sure you set a DefaultValue too.
                         Name = "cake",
                         DefaultValue = "victoria",
                         AutoCompleteOptions = new[] { "carrot", "coffee", "red velvet", "victoria"} // these are the suggestions provided
                     },
                 });

            // This registers a 'property' command handler, i.e. two functions, one for getting, one for setting.
            // You have to specify a name for this property as it can't be inferred from the method names.
            CommandHandlers.RegisterCommandHandler<int>(GetDynamicValue, SetDynamicValue, "DynamicValue");

            // The following demonstrates registering multiple commands to the same method. Useful if you have your
            // own method for handling commands, or want to pass them off to another system (or over the network).
            CommandHandlers.RegisterCommandHandler<object[]>(MultipleCommandHandler, "MultiCommand1", types: new[] { typeof(int), typeof(string) });
            CommandHandlers.RegisterCommandHandler<object[]>(MultipleCommandHandler, "MultiCommand2", types: new[] { typeof(Color) });

            // Of course, you can also register commands with lambda functions, as follows:
            CommandHandlers.RegisterCommandHandler<string>(
                value =>
                {
                    Debug.Log("Call lambda command with value " + value);
                }, 
                "LambdaCommand");

            // As a final resort, you can use this to handle any command that hasn't already been handled.
            // If you use this, you'll only get given a list of strings for arguments, and you won't have any
            // of the autocomplete suggestions. I strongly advise you use the above method if at all possible.
            CommandHandlers.DefaultCommandHandler = DefaultCommandHandler;
        }

        [CommandHandler]
        private static void ParamsMethodTest(int number, params string[] args)
        {
            Debug.Log("Dynamic method called with args: " + number + "  then " + string.Join(",", args));
        }

        private static void DynamicallyRegisteredMethod(string aString, int aNumber)
        {
            Debug.Log("DynamicallyRegisteredMethod called with " + aString + " and " + aNumber);
        }

        private static void DynamicParamsMethod1(int number, params string[] args)
        {
            Debug.Log("Dynamic params method called with args: " + number + "  then " + string.Join(",", args));
        }

        private static void DynamicParamsMethod2(Color color, params object[] args)
        {
            Debug.Log("Dynamic params method called with color " + color + " and args: " + string.Join(",", args.Select(x => x.ToString()).ToArray()));
        }

        // This int and the following two methods behave as a property
        private static int _value;

        private static int GetDynamicValue()
        {
            return _value;
        }

        private static void SetDynamicValue(int value)
        {
            _value = value;
        }

        private static void MultipleCommandHandler(params object[] args)
        {
            switch (CommandHandlers.CurrentExecutingCommand)
            {
                case "MultiCommand1":
                    Debug.Log("MultiCommand1 executed with params " + string.Join(",", args.Select(x => x.ToString()).ToArray()));
                    break;
                case "MultiCommand2":
                    Debug.Log("MultiCommand2 executed with params " + string.Join(",", args.Select(x => x.ToString()).ToArray()));
                    break;
            }
        }

        private bool DefaultCommandHandler(string commandName, string[] args)
        {
            if (commandName == "ADefaultCommand")
            {
                Debug.Log("Handling a default command with parameters " + String.Join(",", args));
                return true;
            }
            return false;
        }
    }
}
