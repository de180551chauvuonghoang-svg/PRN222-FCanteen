using System;
using FCanteen.Data;

namespace FCanteen.Services.Lifetimes
{
    public interface ITransientService { Guid InstanceId { get; } }
    public interface IScopedService { Guid InstanceId { get; } }
    public interface ISingletonService { Guid InstanceId { get; } }

    public class TransientService : ITransientService
    {
        public Guid InstanceId { get; } = Guid.NewGuid();
    }

    public class ScopedService : IScopedService
    {
        public Guid InstanceId { get; } = Guid.NewGuid();
    }

    public class SingletonService : ISingletonService
    {
        public Guid InstanceId { get; } = Guid.NewGuid();
    }

    // Class dung de co tinh tao loi Captive Dependency (Singleton chua DbContext scoped)
    public class CaptiveServiceDemo
    {
        private readonly FCanteenContext _context;
        public CaptiveServiceDemo(FCanteenContext context)
        {
            _context = context;
        }
        public string GetStatus() => "Captive dependency created successfully!";
    }
}