using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using WebApplication1.Exceptions;

namespace WebApplication1.Filters;

public class ExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ExceptionFilter> _logger;
        private readonly GlobalExceptionFilterOptions _options;

        public ExceptionFilter(ILogger<ExceptionFilter> logger,
            IOptionsSnapshot<GlobalExceptionFilterOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.Exception is NotFoundEntityException or FileNotFoundException)
            {
                context.Result = new NotFoundResult();
            }
            else if (context.Exception is ValidationException)
            {
                context.Result = new BadRequestObjectResult(context.Exception.Message);
            }
            else
            {
                _logger.LogError(context.Exception, $"[{DateTime.UtcNow.Ticks}-{Thread.CurrentThread.ManagedThreadId}]");

                if (_options.DetailLevel == ExceptionDetailLevel.Throw)
                {
                    return;
                }

                context.Result = new ObjectResult(new { Message = GetErrorMessage(context.Exception) })
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                };
            }
        }

        private string GetErrorMessage(Exception ex)
        {
            return _options.DetailLevel switch
            {
                ExceptionDetailLevel.None => "An internal exception has occurred.",
                ExceptionDetailLevel.Message => ex.Message,
                ExceptionDetailLevel.StackTrace => ex.StackTrace,
                ExceptionDetailLevel.ToString => ex.ToString(),
                _ => "An internal exception has occurred.",
            };
        }
    }

    public class GlobalExceptionFilterOptions
    {
        public ExceptionDetailLevel DetailLevel { get; set; }
    }

    public enum ExceptionDetailLevel
    {
        None,
        Message,
        StackTrace,
        ToString,
        Throw,
    }