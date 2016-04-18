namespace MyCQRS.Bus.Direct
{
    public interface IRouterMessages
    {
        void Route(object message);
    }
}