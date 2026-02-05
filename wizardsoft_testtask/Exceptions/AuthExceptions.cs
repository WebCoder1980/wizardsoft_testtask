using System;

namespace wizardsoft_testtask.Exceptions
{
    public class AuthException : Exception
    {
        public string Code { get; }

        public AuthException(string code, string message)
            : base(message)
        {
            Code = code;
        }
    }

    public class InvalidCredentialsException : AuthException
    {
        public InvalidCredentialsException()
            : base("invalid_credentials", "Неверный логин или пароль")
        {
        }
    }

    public class UserAlreadyExistsException : AuthException
    {
        public UserAlreadyExistsException()
            : base("user_already_exists", "Пользователь уже занят")
        {
        }
    }
}

