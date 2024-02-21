using Application.Common;
using Application.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ResponseGeneratorService : IResponseGeneratorService
    {
        public async Task<ReturnResponse> GenerateResponseAsync(bool status, int statusCode, string message)
        {
            return new ReturnResponse
            {
                Status = status,
                StatusCode = statusCode,
                Message = message
            };
        }

        public async Task<ReturnResponse<T>> GenerateResponseAsync<T>(bool status, int statusCode, string message, T data)
        {
            return new ReturnResponse<T>
            {
                Status = status,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }
    }

}
