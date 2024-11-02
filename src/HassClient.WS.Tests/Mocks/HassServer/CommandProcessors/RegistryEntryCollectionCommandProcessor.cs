﻿using Bogus;
using HassClient.Models;
using HassClient.Serialization;
using HassClient.WS.Messages;
using HassClient.WS.Messages.Commands;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Reflection;

namespace HassClient.WS.Tests.Mocks.HassServer
{
    public class RegistryEntryCollectionCommandProcessor<TFactory, TModel> : BaseCommandProcessor
        where TFactory : RegistryEntryCollectionMessagesFactory<TModel>
        where TModel : RegistryEntryBase
    {
        protected TFactory modelFactory;

        protected PropertyInfo idPropertyInfo;

        protected Faker faker;

        protected string modelIdPropertyName;

        protected string apiPrefix;

        protected string modelName;

        private bool isContextReady;

        public RegistryEntryCollectionCommandProcessor()
            : this(Activator.CreateInstance<TFactory>())
        {
        }

        protected RegistryEntryCollectionCommandProcessor(TFactory factory)
        {
            this.modelFactory = factory;
            this.faker = new Faker();

            this.modelIdPropertyName = $"{modelFactory.ModelName}_id";
            this.apiPrefix = this.modelFactory.ApiPrefix;
            this.modelName = this.modelFactory.ModelName;
            this.idPropertyInfo = this.GetModelIdPropertyInfo();
        }

        public override bool CanProcess(BaseIdentifiableMessage receivedCommand) =>
            receivedCommand is RawCommandMessage &&
            receivedCommand.Type.StartsWith(this.apiPrefix) &&
            this.IsValidCommandType(receivedCommand.Type);

        public override BaseIdentifiableMessage ProcessCommand(MockHassServerRequestContext context, BaseIdentifiableMessage receivedCommand)
        {
            try
            {
                if (!this.isContextReady)
                {
                    this.isContextReady = true;
                    this.PrepareHassContext(context);
                }

                var merged = (receivedCommand as RawCommandMessage).MergedObject as JToken;
                var commandType = receivedCommand.Type;
                object result = null;

                if (commandType.EndsWith("list"))
                {
                    result = this.ProcessListCommand(context, merged);
                }
                else if (commandType.EndsWith("create"))
                {
                    result = this.ProcessCreateCommand(context, merged);
                }
                else if (commandType.EndsWith("delete"))
                {
                    result = this.ProcessDeleteCommand(context, merged);
                }
                else if (commandType.EndsWith("update"))
                {
                    result = (object)this.ProcessUpdateCommand(context, merged) ?? ErrorCodes.NotFound;
                }
                else
                {
                    result = this.ProcessUnknownCommand(commandType, context, merged);
                }

                if (result is ErrorCodes errorCode)
                {
                    return this.CreateResultMessageWithError(new ErrorInfo(errorCode));
                }
                else
                {
                    var resultObject = new JRaw(HassSerializer.SerializeObject(result));
                    return this.CreateResultMessageWithResult(resultObject);
                }
            }
            catch (Exception ex)
            {
                return this.CreateResultMessageWithError(new ErrorInfo(ErrorCodes.UnknownError) { Message = ex.Message });
            }
        }

        protected virtual PropertyInfo GetModelIdPropertyInfo()
        {
            var modelType = typeof(TModel);
            var properties = modelType.GetProperties();
            var modelIdProperty = properties.FirstOrDefault(x => HassSerializer.GetSerializedPropertyName(x) == this.modelIdPropertyName);
            return modelIdProperty ?? properties.Where(x => x.Name.EndsWith("Id")).OrderBy(x => x.Name.Length).FirstOrDefault();
        }

        protected virtual bool IsValidCommandType(string commandType)
        {
            return commandType.EndsWith("create") ||
                   commandType.EndsWith("list") ||
                   commandType.EndsWith("update") ||
                   commandType.EndsWith("delete");
        }

        private string GetModelSerialized(JToken merged)
        {
            string modelSerialized = HassSerializer.SerializeObject(merged);
            var idPropertyName = HassSerializer.GetSerializedPropertyName(idPropertyInfo);
            if (this.modelIdPropertyName != idPropertyName)
            {
                modelSerialized = modelSerialized.Replace(this.modelIdPropertyName, idPropertyName);
            }

            return modelSerialized;
        }

        protected virtual TModel DeserializeModel(JToken merged, out string modelSerialized)
        {
            modelSerialized = this.GetModelSerialized(merged);
            return HassSerializer.DeserializeObject<TModel>(modelSerialized);
        }

        protected virtual void PopulateModel(JToken merged, object target)
        {
            var modelSerialized = this.GetModelSerialized(merged);
            HassSerializer.PopulateObject(modelSerialized, target);
        }

        protected virtual object ProcessListCommand(MockHassServerRequestContext context, JToken merged)
        {
            return context.HassDB.GetObjects<TModel>();
        }

        protected virtual object ProcessCreateCommand(MockHassServerRequestContext context, JToken merged)
        {
            var model = this.DeserializeModel(merged, out var _);
            this.idPropertyInfo.SetValue(model, this.faker.RandomUUID());
            context.HassDB.CreateObject(model);
            return model;
        }

        protected virtual object ProcessUpdateCommand(MockHassServerRequestContext context, JToken merged)
        {
            var model = this.DeserializeModel(merged, out var modelSerialized);
            return context.HassDB.UpdateObject(model, new JRaw(modelSerialized));
        }

        protected virtual object ProcessDeleteCommand(MockHassServerRequestContext context, JToken merged)
        {
            var model = this.DeserializeModel(merged, out var _);
            context.HassDB.DeleteObject(model);
            return null;
        }

        protected virtual object ProcessUnknownCommand(string commandType, MockHassServerRequestContext context, JToken merged)
        {
            return ErrorCodes.NotSupported;
        }

        protected virtual void PrepareHassContext(MockHassServerRequestContext context)
        {
        }
    }
}
