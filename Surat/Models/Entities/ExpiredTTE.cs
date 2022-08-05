using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Surat.Models.Entities
{
    public class ExpiredTTE
    {
        public string Id { get; set; }
        public string Nama { get; set; }
        public string Nip { get; set; }
        public string Email { get; set; }
        public string JenisSertifikat { get; set; }
        public DateTime TanggalKadaluarsa { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    /* 	ID VARCHAR(50) NOT NULL PRIMARY KEY,
	NAMA VARCHAR(255),
	NIP VARCHAR(255),
	EMAIL VARCHAR(100),
	JENISSERTIFIKAT VARCHAR(100),
	TANGGALKADALUARSA DATE,
	CREATEDDATE DATE,
	UPDATEDDATE DATE */

    public class ExpiredTTENotif
    {
        public string Tanggal { get; set; }
        public bool Show { get; set; }
    }
}