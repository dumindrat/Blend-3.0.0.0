using System;

namespace Blend.Data
{
    public abstract class BaseEntity
    {
        //public long Id { get; set; }
        public string AddedDate { get; set; }
        public string ModifiedDate { get; set; }
        public string RowVersion { get; set; }
        public string OperationCommand { get; set; }
    }
}
