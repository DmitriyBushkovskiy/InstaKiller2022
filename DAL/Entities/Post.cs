using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = null!;
        public DateTimeOffset Created { get; set; }
        public bool Changed { get; set; } = false;
        public Guid AuthorID { get; set; }
        public virtual User Author { get; set; } = null!;
        public virtual List <PostContent> Content { get; set; } = null!;
        //TODO: добавить дополнительные поля:
        // int Likes
    }
}
