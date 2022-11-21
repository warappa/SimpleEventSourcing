namespace Shop.UI.Web.Shared
{
    public class ExceptionDto
    {
        public string Message { get; set; }
        public ErrorDto[] Errors { get; set; }
        public string Name { get; set; }

        public class ErrorDto
        {
            public string Key { get; set; }
            public string Message { get; set; }
        }
    }
}
