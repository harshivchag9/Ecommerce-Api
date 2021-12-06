using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectApi.Models
{
    [Keyless]
    public partial class ResponseMessage
    {
        public ResponseMessage() { }
        public ResponseMessage(int id, string responseBool, int responseNumber, string msg)
        {
            this.ResponseId = id;
            this.ResponseBool = responseBool;
            this.ResponseNumber = responseNumber;
            this.ResponseMsg = msg;
        }
        public int ResponseId { get; set; }
        public string ResponseBool { get; set; }
        public int ResponseNumber { get; set; }
        public string ResponseMsg { get; set; }


    }
}