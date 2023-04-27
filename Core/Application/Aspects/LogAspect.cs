using Application.Utilities.Interceptors;
using Application.Utilities.IoC;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Aspects
{
    public class LogAspect : MethodInterception
    {
        private string? onBeforeMessage;
        private string? onAfterMessage;
        public bool isDate;
        private ILogger logger;

        public LogAspect(string? onBeforeMessage, string? onAfterMessage)
        {
            this.onBeforeMessage = onBeforeMessage;
            this.onAfterMessage = onAfterMessage;
            logger = ServiceTool.ServiceProvider.GetRequiredService<ILogger>();
        }

        public LogAspect(string? onBeforeMessage)
        {
            this.onBeforeMessage = onBeforeMessage;
            logger = ServiceTool.ServiceProvider.GetRequiredService<ILogger>();
        }

        public LogAspect(string? onBeforeMessage, string? onAfterMessage, bool isDate) : this(onBeforeMessage, onAfterMessage)
        {
            this.isDate = isDate;
            logger = ServiceTool.ServiceProvider.GetRequiredService<ILogger>();
        }

        public LogAspect(string? onBeforeMessage, bool isDate)
        {
            this.isDate= isDate;
            this.onBeforeMessage = onBeforeMessage;
            logger = ServiceTool.ServiceProvider.GetRequiredService<ILogger>();
        }

        public LogAspect()
        {
            logger = ServiceTool.ServiceProvider.GetRequiredService<ILogger>();
        }

        public LogAspect(bool isDate)
        {
            this.isDate = isDate;
            logger = ServiceTool.ServiceProvider.GetRequiredService<ILogger>();
        }

        protected override void OnBefore(IInvocation invocation)
        {
            if (onBeforeMessage != null)
            {
                if (isDate)
                {
                    logger.LogInformation($"{DateTime.Now} tarihinde - {onBeforeMessage}");
                }
                else
                {
                    logger.LogInformation(onBeforeMessage);
                }          
            }
            else if (onBeforeMessage == null && isDate)
            {
                logger.LogInformation($"{DateTime.Now} tarihinde - on before log");
            }
            else if (onBeforeMessage == null && !isDate)
            {
                logger.LogInformation("on before log");
            }
        }

        protected override void OnAfter(IInvocation invocation)
        {
            if (onAfterMessage != null)
            {
                if (isDate)
                {
                    logger.LogInformation($"{DateTime.Now} tarihinde - {onAfterMessage}");
                }
                else
                {
                    logger.LogInformation(onBeforeMessage);
                }
            }
            else if (onAfterMessage == null && isDate)
            {
                logger.LogInformation($"{DateTime.Now} tarihinde - on after log");
            }
            else if (onAfterMessage == null && !isDate)
            {
                logger.LogInformation("on after log");
            }
        }
    }
}