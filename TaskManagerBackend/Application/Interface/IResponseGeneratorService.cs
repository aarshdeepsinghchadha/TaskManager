using Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface IResponseGeneratorService
    {
        Task<ReturnResponse> GenerateResponseAsync(bool status, int statusCode, string message);

        Task<ReturnResponse<T>> GenerateResponseAsync<T>(bool status, int statusCode, string message, T data);
    }
}
