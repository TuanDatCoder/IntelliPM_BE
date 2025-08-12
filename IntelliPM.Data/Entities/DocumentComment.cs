using System;
using System.Collections.Generic;

namespace IntelliPM.Data.Entities;

public partial class DocumentComment
{
    public int Id { get; set; }

    public int DocumentId { get; set; }

    public int AuthorId { get; set; }

    public int FromPos { get; set; }

    public int ToPos { get; set; }

    public string Content { get; set; } = null!;

    public string Comment { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account Author { get; set; } = null!;

    public virtual Document Document { get; set; } = null!;
}
