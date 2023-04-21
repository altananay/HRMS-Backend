using System.Text;

namespace Application.Aspects.CustomAspects.Aspects
{
    public class LogAspect : AspectBase, IBeforeVoidAspect
    {
        public void OnBefore()
        {
            StringBuilder logMessage = new StringBuilder();

            logMessage.AppendLine(string.Format("Method name: {0}", AspectContext.Instance.MethodName));

            logMessage.AppendLine(string.Format("Arguments: {0}", string.Join(",", AspectContext.Instance.Arguments)));

            //todo: mongodb logging

        }
    }
}