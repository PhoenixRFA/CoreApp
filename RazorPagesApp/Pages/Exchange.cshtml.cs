using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPagesApp.Pages
{
    public class ExchangeModel : PageModel
    {
        public string Message { get; set; }
        public decimal Rate { get; set; } = 73.39m;

        public void OnGet()
        {
            Message = "Enter sum";
        }

        public void OnPost(int? sum)
        {
            if (sum == null || sum <= 0)
            {
                Message = "Wrong amount!";
            }
            else
            {
                decimal res = (decimal)sum / Rate;
                Message = $"Result: {sum} RUB = {res:F2} USD";
            }
        }
    }
}
