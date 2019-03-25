using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Loom.ZombieBattleground
{
    public static class TutorialExtentions
    {
        public static TutorialGameplayStep ToGameplayStep(this TutorialStep step)
        {
            return step as TutorialGameplayStep;
        }

        public static TutorialMenuStep ToMenuStep(this TutorialStep step)
        {
            return step as TutorialMenuStep;
        }

        public static TutorialGameplayContent ToGameplayContent(this TutorialContent content)
        {
            return content as TutorialGameplayContent;
        }

        public static TutorialMenusContent ToMenusContent(this TutorialContent content)
        {
            return content as TutorialMenusContent;
        }
    }

    public class TutorialStepConverter : JsonCreationConverter<TutorialStep>
    {
        protected override TutorialStep Create(Type objectType, JObject jsonObject)
        {
            Common.Enumerators.TutorialStepType tutorialStepType = Common.Enumerators.TutorialStepType.MenuStep;

            if (jsonObject.TryGetValue("TutorialStepType", out JToken value))
            {
               if(!Enum.TryParse(value.ToObject<string>(), out tutorialStepType))
               {
                    Helpers.ExceptionReporter.SilentReportException(new Exception($"parse value: {value.ToObject<string>()} from tutorial data error"));
               }
            }

            if (tutorialStepType == Common.Enumerators.TutorialStepType.MenuStep)
            {
                return new TutorialMenuStep();
            }
            else
            {
                return new TutorialGameplayStep();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonArray = JArray.Load(reader);

            List<TutorialStep> steps = new List<TutorialStep>();

            for (int i = 0; i < jsonArray.Count; i++)
            {
                var jsonObject = (JObject)jsonArray[i];
                var target = Create(objectType, jsonObject);
                serializer.Populate(jsonObject.CreateReader(), target);
                steps.Add(target);
            }

            return steps;
        }
    }

    public class TutorialContentConverter : JsonCreationConverter<TutorialContent>
    {
        protected override TutorialContent Create(Type objectType, JObject jsonObject)
        {
            if (jsonObject["SpecificBattlegroundInfo"] != null)
            {
                return new TutorialGameplayContent();
            }
            else if (jsonObject["HasBlockedInteractivityInGameScreens"] != null)
            {
                return new TutorialMenusContent();
            }

            return null;
        }
    }

    public class TutorialActivityActionHandlerDataConverter : JsonCreationConverter<TutorialActivityActionHandlerData>
    {
        protected override TutorialActivityActionHandlerData Create(Type objectType, JObject jsonObject)
        {
            if (jsonObject["TutorialTooltipAlign"] != null)
            {
                return new OverlordSayTooltipInfo();
            }
            else if (jsonObject["TutorialDescriptionTooltipsToActivate"] != null)
            {
                return new DrawDescriptionTooltipsInfo();
            }

            return null;
        }
    }

    public abstract class JsonCreationConverter<T> : JsonConverter
    {
        protected abstract T Create(Type objectType, JObject jsonObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var target = Create(objectType, jsonObject);
            serializer.Populate(jsonObject.CreateReader(), target);
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }
    }
}
