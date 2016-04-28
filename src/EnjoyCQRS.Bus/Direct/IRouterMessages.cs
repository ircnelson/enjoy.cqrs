namespace EnjoyCQRS.Bus.Direct
{
    public interface IRouterMessages
    {
        void Route(object message);
    }
}