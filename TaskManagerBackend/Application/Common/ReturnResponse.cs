using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common
{
    public class ReturnResponse
    {
        public bool Status { get; set; }
        public int StatusCode { get; set; }
        public string? Message { get; set; }


    }
    public class ReturnResponse<T> : ReturnResponse
    {
        public T? Data { get; set; }
    }
    
    public class PagedResponse<T> : ReturnResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }

        public PagedResponse( T data , int pageNumber, int pageSize) 
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            Data = data;  
        }
    }
}
