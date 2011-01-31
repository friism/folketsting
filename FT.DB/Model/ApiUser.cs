using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;


namespace FT.DB
{
    [MetadataType(typeof(ApiUserValidation))]
    public partial class ApiUser
    {
        public class ApiUserValidation
        {
            [Required]
            [RegularExpression(@"^(([A-Za-z0-9]+_+)|([A-Za-z0-9]+\-+)|([A-Za-z0-9]+\.+)|([A-Za-z0-9]+\++))*[A-Za-z0-9]+@((\w+\-+)|(\w+\.))*\w{1,63}\.[a-zA-Z]{2,6}$", 
                ErrorMessage = "Ikke en ægte email adresse")] 
            public string EmailAddress { get; set; }
        }
    }
}
