namespace wizardsoft_testtask.Exceptions
{
    public class CircleInTreeException : AuthException
    {
        public CircleInTreeException()
            : base("circle_in_tree", "В дереве не может быть циклов")
        {
        }
    }
}
