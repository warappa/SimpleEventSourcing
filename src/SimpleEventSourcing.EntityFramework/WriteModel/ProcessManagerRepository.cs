﻿using SimpleEventSourcing.Domain;
using SimpleEventSourcing.WriteModel;
using System;

namespace SimpleEventSourcing.EntityFramework.WriteModel
{
    public class ProcessManagerRepository : IProcessManagerRepository
    {
        private readonly IEventRepository repository;

        public ProcessManagerRepository(IEventRepository repository)
        {
            this.repository = repository;
        }

        public IProcessManager Get(Type processManagerType, string processId)
        {
            return (IProcessManager)repository.GetAsync(processManagerType, processId);
        }

        public void Save(IProcessManager processManager)
        {
            repository.SaveAsync(processManager);
        }
    }
}
