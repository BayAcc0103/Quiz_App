using BlazingQuiz.Shared;

namespace BlazingQuiz.Web
{
    public class WebPlatform : IPlatform
    {
        public bool IsMobileApp => false;

        public bool IsWeb => true;
    }
}
