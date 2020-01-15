using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SSD2019.Models
{
    [Table("ordini")] //indico che la classe che sto definendo è la corrispondente nel db della tabella ordini
	public class Order
	{
		//[Key, Column("PK_UID", TypeName = "INTEGER")]
		//public long pk_uid { get; set; }
      [Column("id", TypeName = "INTEGER")]
      public long id { get; set; }
      [Column("customer", TypeName = "VARCHAR")]
      public string customer { get; set; }
      [Column("time", TypeName = "INTEGER")]
      public int time { get; set; }
      [Column("quant", TypeName = "INTEGER")]
      public int quant { get; set; }

        public Order(long id, string customer, int time, int quant)
        {
            this.id = id;
            this.customer = customer;
            this.time = time;
            this.quant = quant;
        }
   }
}