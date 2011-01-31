using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;


namespace FT.DB
{
    public enum QuestionType { Politician = 0, User };

    [MetadataType(typeof(P20QuestionValidation))]
    public partial class P20Question
    {
        // this is to get around L2S update crap. It's pretty dodgy
        public Politician AskerPol
        {
            get { return this.Politician; }
            set { this.Politician = value; }
        }

        public Politician AskeePol
        {
            get { return this.Politician1; }
            set { this.Politician1 = value; }
        }

        public User AskerUser
        {
            get { return this.User; }
            set { this.User = value; }
        }

        public class P20QuestionValidation
        {
            [Required]
            [StringLength(80)]
            public string Title { get; set; }

            [Required]
            [StringLength(200)]
            public string Question { get; set; }

            [Required]
            [StringLength(600)]
            public string Background { get; set; }

            [Required]
            public string AskeeTitle { get; set; }

            [Required]
            public DateTime AskDate { get; set; }
        }

    }
}
