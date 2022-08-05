using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using Surat.Codes;
using Surat.Models.Entities;
using static Surat.Codes.Functions;

namespace Surat.Models
{
	public class PenomoranModel
	{
		string skema = OtorisasiUser.NamaSkema;


		public TransactionResult SimpanBukuPenomoran(BukuPenomran data, userIdentity usr)
		{
			var tr = new TransactionResult() { Status = false, Pesan = "" };
			string skema = OtorisasiUser.NamaSkema;
			using (var ctx = new PostgresDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{
						string sql = string.Empty;
						var arrayListParameters = new ArrayList();
						var parameters = arrayListParameters.OfType<object>().ToArray();

						if (!data.Update)
						{
							//insert table bukunomor
							arrayListParameters = new ArrayList();
							sql = $"INSERT INTO \"{skema}\".BUKUNOMOR (BUKUNOMORID, NAMA, STATUSAKTIF) VALUES (:bukunomorid,:nama,:statusaktif)";
							arrayListParameters.Add(new NpgsqlParameter("bukunomorid", data.BukuNomorId));
							arrayListParameters.Add(new NpgsqlParameter("nama", data.Nama));
							arrayListParameters.Add(new NpgsqlParameter("statusaktif", data.StatusAktif));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);

							//insert table tblpenandatanganbuku
							foreach (var penandatangan in data.ListPenandatanganBuku)
							{
								arrayListParameters.Clear();
								sql = $"INSERT INTO \"{skema}\".TBLPENANDATANGANBUKU (BUKUNOMORID, PROFILEID, UNITKERJAID, STATUSAKTIF, NAMA) VALUES (:bukunomorid, :profileid, :unitkerjaid, :statusaktif, :jabatannama)";
								arrayListParameters.Add(new NpgsqlParameter("bukunomorid", penandatangan.BukuNomorId));
								arrayListParameters.Add(new NpgsqlParameter("profileid", penandatangan.ProfileId));
								arrayListParameters.Add(new NpgsqlParameter("unitkerjaid", penandatangan.UnitKerjaId));
								arrayListParameters.Add(new NpgsqlParameter("statusaktif", penandatangan.StatusAktif));
								arrayListParameters.Add(new NpgsqlParameter("jabatannama", penandatangan.JabatanNama));
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql, parameters);
							}

							//insert table aksesbuku
							foreach (var akses in data.ListAksesBuku)
							{
								arrayListParameters.Clear();
								sql = $"INSERT INTO \"{skema}\".TBLAKSESBUKU (BUKUNOMORID, UNITKERJAID, STATUSAKTIF,PEGAWAIID, NAMA) VALUES (:bukunomorid, :unitkerjaid, :statusaktif, :pegawaiid, :namapegawai)";
								arrayListParameters.Add(new NpgsqlParameter("bukunomorid", akses.BukuNomorId));
								arrayListParameters.Add(new NpgsqlParameter("unitkerjaid", akses.UnitKerjaId));
								arrayListParameters.Add(new NpgsqlParameter("statusaktif", akses.StatusAktif));
								arrayListParameters.Add(new NpgsqlParameter("pegawaiid", akses.PegawaiId));
								arrayListParameters.Add(new NpgsqlParameter("namapegawai", akses.Nama));
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql, parameters);
							}
						}
						else
						{
							//panggil data lama
							var oldata = getListBukuPenomoran(usr.PegawaiId, usr.UnitKerjaId, data.BukuNomorId)[0];

							if (data.Nama != oldata.Nama)
							{
								//update table bukunomor
								arrayListParameters = new ArrayList();
								sql = $"UPDATE \"{skema}\".BUKUNOMOR SET NAMA =:nama WHERE BUKUNOMORID = :bukunomorid";
								arrayListParameters.Add(new NpgsqlParameter("nama", data.Nama));
								arrayListParameters.Add(new NpgsqlParameter("bukunomorid", data.BukuNomorId));
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql, parameters);
							}

							//Penandatangan - cari dan Update eksisting dan tambah baru bila ada
							var deletedDataPenandatangan = oldata.ListPenandatanganBuku;
							int thisindex = 0;
							foreach (var p in data.ListPenandatanganBuku)
							{
								if (oldata.ListPenandatanganBuku.Exists(x => x.ProfileId == p.ProfileId))
								{
									var thispenandatangan = oldata.ListPenandatanganBuku.Find(x => x.ProfileId == p.ProfileId);
									var stringUpdate = "";

									arrayListParameters = new ArrayList();

									if (thispenandatangan.UnitKerjaId != p.UnitKerjaId)
									{
										stringUpdate += (string.IsNullOrEmpty(stringUpdate)) ? " UNITKERJAID = :unitkerjaid " : " ,UNITKERJAID = :unitkerjaid ";
										arrayListParameters.Add(new NpgsqlParameter("unitkerjaid", p.UnitKerjaId));
									}

									if (thispenandatangan.StatusAktif != p.StatusAktif)
									{
										stringUpdate += (string.IsNullOrEmpty(stringUpdate)) ? " STATUSAKTIF = :statusaktif " : " ,STATUSAKTIF = :statusaktif ";
										arrayListParameters.Add(new NpgsqlParameter("statusaktif", p.StatusAktif));
									}

									if (thispenandatangan.JabatanNama != p.JabatanNama)
									{
										stringUpdate += (string.IsNullOrEmpty(stringUpdate)) ? " NAMA = :jabatannama " : " ,NAMA = :jabatannama ";
										arrayListParameters.Add(new NpgsqlParameter("jabatannama", p.JabatanNama));
									}

									if (!string.IsNullOrEmpty(stringUpdate))
									{
										sql = $"UPDATE \"{skema}\".TBLPENANDATANGANBUKU SET {stringUpdate} WHERE BUKUNOMORID = :bukunomorid AND PROFILEID = :profileid";
										arrayListParameters.Add(new NpgsqlParameter("bukunomorid", p.BukuNomorId));
										arrayListParameters.Add(new NpgsqlParameter("profileid", p.ProfileId));
										parameters = arrayListParameters.OfType<object>().ToArray();
										ctx.Database.ExecuteSqlCommand(sql, parameters);
									}

									deletedDataPenandatangan.Remove(thispenandatangan);
								}
								else
								{
									arrayListParameters = new ArrayList();
									sql = $"INSERT INTO \"{skema}\".TBLPENANDATANGANBUKU (BUKUNOMORID, PROFILEID, UNITKERJAID, STATUSAKTIF, NAMA) VALUES (:bukunomorid, :profileid, :unitkerjaid, :statusaktif, :jabatannama)";
									arrayListParameters.Add(new NpgsqlParameter("bukunomorid", p.BukuNomorId));
									arrayListParameters.Add(new NpgsqlParameter("profileid", p.ProfileId));
									arrayListParameters.Add(new NpgsqlParameter("unitkerjaid", p.UnitKerjaId));
									arrayListParameters.Add(new NpgsqlParameter("statusaktif", p.StatusAktif));
									arrayListParameters.Add(new NpgsqlParameter("jabatannama", p.JabatanNama));
									parameters = arrayListParameters.OfType<object>().ToArray();
									ctx.Database.ExecuteSqlCommand(sql, parameters);
								}
								thisindex++;
							}

							if (deletedDataPenandatangan.Count > 0)
							{
								arrayListParameters = new ArrayList();
								string listStrProfileid = "";
								int param = 1;
								foreach (var d in deletedDataPenandatangan)
								{
									listStrProfileid += (string.IsNullOrEmpty(listStrProfileid)) ? $":param{param}" : $",:param{param}";
									param++;
								}
								sql = $"UPDATE \"{skema}\".TBLPENANDATANGANBUKU SET STATUSAKTIF = '0' WHERE BUKUNOMORID = :bukunomorid AND PROFILEID IN ({listStrProfileid})";
								arrayListParameters.Add(new NpgsqlParameter("bukunomorid", data.BukuNomorId));
								param = 1;
								foreach (var par in deletedDataPenandatangan)
								{
									arrayListParameters.Add(new NpgsqlParameter($":param{param}", par.ProfileId));
									param++;
								}
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql, parameters);
							}


							//Akses - cari dan Update eksisting dan tambah baru bila ada
							var deletedDataAkses = oldata.ListAksesBuku;
							thisindex = 0;
							foreach (var a in data.ListAksesBuku)
							{
								if (oldata.ListAksesBuku.Exists(x => x.PegawaiId == a.PegawaiId))
								{
									var thisAkses = oldata.ListAksesBuku.Find(x => x.PegawaiId == a.PegawaiId);
									var stringUpdate = "";

									arrayListParameters = new ArrayList();

									if (thisAkses.UnitKerjaId != a.UnitKerjaId)
									{
										stringUpdate += (string.IsNullOrEmpty(stringUpdate)) ? " UNITKERJAID = :unitkerjaid " : " ,UNITKERJAID = :unitkerjaid ";
										arrayListParameters.Add(new NpgsqlParameter("unitkerjaid", a.UnitKerjaId));
									}

									if (thisAkses.StatusAktif != a.StatusAktif)
									{
										stringUpdate += (string.IsNullOrEmpty(stringUpdate)) ? " STATUSAKTIF = :statusaktif " : " ,STATUSAKTIF = :statusaktif ";
										arrayListParameters.Add(new NpgsqlParameter("statusaktif", a.StatusAktif));
									}

									if (thisAkses.Nama != a.Nama)
									{
										stringUpdate += (string.IsNullOrEmpty(stringUpdate)) ? " NAMA = :namapegawai " : " ,NAMA = :namapegawai ";
										arrayListParameters.Add(new NpgsqlParameter("namapegawai", a.Nama));
									}

									if (!string.IsNullOrEmpty(stringUpdate))
									{
										sql = $"UPDATE \"{skema}\".TBLAKSESBUKU SET {stringUpdate} WHERE BUKUNOMORID = :bukunomorid AND PEGAWAIID = :pegawaiid";
										arrayListParameters.Add(new NpgsqlParameter("bukunomorid", a.BukuNomorId));
										arrayListParameters.Add(new NpgsqlParameter("pegawaiid", a.PegawaiId));
										parameters = arrayListParameters.OfType<object>().ToArray();
										ctx.Database.ExecuteSqlCommand(sql, parameters);
									}

									deletedDataAkses.Remove(thisAkses);
								}
								else
								{
									arrayListParameters.Clear();
									sql = $"INSERT INTO \"{skema}\".TBLAKSESBUKU (BUKUNOMORID, UNITKERJAID, STATUSAKTIF,PEGAWAIID, NAMA) VALUES (:bukunomorid, :unitkerjaid, :statusaktif, :pegawaiid, :namapegawai)";
									arrayListParameters.Add(new NpgsqlParameter("bukunomorid", a.BukuNomorId));
									arrayListParameters.Add(new NpgsqlParameter("unitkerjaid", a.UnitKerjaId));
									arrayListParameters.Add(new NpgsqlParameter("statusaktif", a.StatusAktif));
									arrayListParameters.Add(new NpgsqlParameter("pegawaiid", a.PegawaiId));
									arrayListParameters.Add(new NpgsqlParameter("namapegawai", a.Nama));
									parameters = arrayListParameters.OfType<object>().ToArray();
									ctx.Database.ExecuteSqlCommand(sql, parameters);
								}
								thisindex++;
							}

							if (deletedDataAkses.Count > 0)
							{
								arrayListParameters = new ArrayList();
								string listStrProfileid = "";
								int param = 1;
								foreach (var d in deletedDataAkses)
								{
									listStrProfileid += (string.IsNullOrEmpty(listStrProfileid)) ? $":param{param}" : $",:param{param}";
									param++;
								}
								sql = $"UPDATE \"{skema}\".TBLAKSESBUKU SET STATUSAKTIF = '0' WHERE BUKUNOMORID = :bukunomorid AND PEGAWAIID IN ({listStrProfileid})";
								arrayListParameters.Add(new NpgsqlParameter("bukunomorid", data.BukuNomorId));
								param = 1;
								foreach (var par in deletedDataAkses)
								{
									arrayListParameters.Add(new NpgsqlParameter($":param{param}", par.PegawaiId));
									param++;
								}
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql, parameters);
							}
						}

						tc.Commit();
						tr.Status = true;
						tr.Pesan = "Data Berhasil Disimpan";
					}
					catch (Exception ex)
					{
						tc.Rollback();
						tr.Pesan = ex.Message.ToString();
					}
					finally
					{
						tc.Dispose();
						ctx.Dispose();
					}
				}
			}
			return tr;
		}

		public TransactionResult CancelPenomoran(string penomoranid)
		{
			var tr = new TransactionResult() { Status = false, Pesan = "Gagal Menghapus Buku" };
			string skema = OtorisasiUser.NamaSkema;
			var arrayListParameters = new ArrayList();
			var parameters = arrayListParameters.OfType<object>().ToArray();
			if (!string.IsNullOrEmpty(penomoranid))
			{
				string sql = $@"UPDATE ""{skema}"".TBLPENOMORAN
							SET STATUSBATAL = '1' WHERE PENOMORANID = :penomoranid";
				arrayListParameters.Add(new NpgsqlParameter("bukunomorid", penomoranid));
				parameters = arrayListParameters.OfType<object>().ToArray();

				using (var ctx = new PostgresDbContext())
				{
					using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
					{
						try
						{
							ctx.Database.ExecuteSqlCommand(sql, parameters);
							tc.Commit();
							tr.Status = true;
							tr.Pesan = "Nomor Surat telah dibatalkan";
						}
						catch (Exception ex)
						{
							tc.Rollback();
							tr.Pesan = ex.Message.ToString();
						}
					}
				}
			}
			return tr;
		}

		public List<BukuPenomran> getListBukuPenomoran(string pegawaiid, string unitkerjaid, string bukunomorid = null)
		{
			var datas = new List<BukuPenomran>();
			string skema = OtorisasiUser.NamaSkema;
			var arrayListParameters = new ArrayList();
			var parameters = arrayListParameters.OfType<object>().ToArray();
			var ctx = new PostgresDbContext();
			string sql = $@"SELECT
								BN.BUKUNOMORID ,BN.NAMA, TO_CHAR(BN.CREATED, 'DD/MM/YYYY') AS TANGGALBUAT, BN.STATUSAKTIF
							FROM
								""{skema}"".BUKUNOMOR BN
							INNER JOIN
								""{skema}"".TBLAKSESBUKU TA ON BN.BUKUNOMORID = TA.BUKUNOMORID 
								AND TA.PEGAWAIID = :pegawaiid 
								AND TA.UNITKERJAID = :unitkerjaid 
								AND TA.STATUSAKTIF <> '0'
							WHERE 
								BN.STATUSAKTIF <> '0'";
			if (!string.IsNullOrEmpty(bukunomorid))
			{
				sql += " AND BN.BUKUNOMORID = :bukunomorid ";
				arrayListParameters.Add(new NpgsqlParameter("bukunomorid", bukunomorid));
			}

			sql += " GROUP BY BN.BUKUNOMORID ";

			arrayListParameters.Add(new NpgsqlParameter("pegawaiid", pegawaiid));
			arrayListParameters.Add(new NpgsqlParameter("unitkerjaid", unitkerjaid));
			parameters = arrayListParameters.OfType<object>().ToArray();

			datas = ctx.Database.SqlQuery<BukuPenomran>(sql, parameters).ToList();

			if (datas.Count() > 0)
			{
				foreach (var data in datas)
				{
					var pb = new List<PenandatanganBuku>();
					arrayListParameters.Clear();
					sql = $@"SELECT
								PB.BUKUNOMORID, PB.PROFILEID, PB.UNITKERJAID, PB.STATUSAKTIF, PB.NAMA AS JABATANNAMA
							FROM
								""{skema}"".TBLPENANDATANGANBUKU PB
							WHERE
								PB.BUKUNOMORID = :bukunomorid
								AND PB.STATUSAKTIF <> '0'";
					arrayListParameters.Add(new NpgsqlParameter("bukunomorid", data.BukuNomorId));
					parameters = arrayListParameters.OfType<object>().ToArray();
					pb = ctx.Database.SqlQuery<PenandatanganBuku>(sql, parameters).ToList();
					data.ListPenandatanganBuku = pb;

					var ab = new List<AksesBuku>();
					arrayListParameters.Clear();
					sql = $@"SELECT
								AB.BUKUNOMORID, AB.PEGAWAIID, AB.UNITKERJAID, AB.STATUSAKTIF, AB.NAMA
							FROM
								""{skema}"".TBLAKSESBUKU AB
							WHERE
								AB.BUKUNOMORID = :bukunomorid
								AND AB.STATUSAKTIF <> '0'";
					arrayListParameters.Add(new NpgsqlParameter("bukunomorid", data.BukuNomorId));
					parameters = arrayListParameters.OfType<object>().ToArray();
					ab = ctx.Database.SqlQuery<AksesBuku>(sql, parameters).ToList();
					data.ListAksesBuku = ab;
				}
			}

			return datas;
		}

		public TransactionResult HapusBuku(string bukunomorid)
		{
			var tr = new TransactionResult() { Status = false, Pesan = "Gagal Menghapus Buku" };
			string skema = OtorisasiUser.NamaSkema;
			var arrayListParameters = new ArrayList();
			var parameters = arrayListParameters.OfType<object>().ToArray();
			if (!string.IsNullOrEmpty(bukunomorid) && bukunomorid != "QasKCHstnCfTwgK8jPNRyKEwUB5s90ATRBPN")
			{
				string sql = $@"UPDATE ""{skema}"".BUKUNOMOR
							SET STATUSAKTIF = '0' WHERE BUKUNOMORID = :bukunomorid";
				arrayListParameters.Add(new NpgsqlParameter("bukunomorid", bukunomorid));
				parameters = arrayListParameters.OfType<object>().ToArray();

				using (var ctx = new PostgresDbContext())
				{
					using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
					{
						try
						{
							ctx.Database.ExecuteSqlCommand(sql, parameters);
							tc.Commit();
							tr.Status = true;
							tr.Pesan = "Buku Berhasil Dihapus";
						}
						catch (Exception ex)
						{
							tc.Rollback();
							tr.Pesan = ex.Message.ToString();
						}
					}
				}
			}
			return tr;
		}

		public TransactionResult penyerahanArsip(string penomoranid, userIdentity usr)
		{
			var tr = new TransactionResult() { Status = false, Pesan = "Gagal Merubah status penyerahan" };
			string skema = OtorisasiUser.NamaSkema;
			var arrayListParameters = new ArrayList();
			string sql = $@"UPDATE ""{skema}"".TBLPENOMORAN
							SET STATUS = '1' WHERE PENOMORANID = :penomoranid";
			arrayListParameters.Add(new NpgsqlParameter("penomoranid", penomoranid));
			var parameters = arrayListParameters.OfType<object>().ToArray();
			using (var ctx = new PostgresDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{
						ctx.Database.ExecuteSqlCommand(sql, parameters);

						sql = $@"INSERT INTO ""{skema}"".TBLKETPENOMORAN (PENOMORANID, LABEL, KETVALUE, WAKTU)
								 VALUES (:penomoranId, :label, :ketvalue, :waktu)";
						arrayListParameters = new ArrayList();
						arrayListParameters.Add(new NpgsqlParameter("penomoranId", penomoranid));
						arrayListParameters.Add(new NpgsqlParameter("label", "PenerimaArsip"));
						arrayListParameters.Add(new NpgsqlParameter("ketvalue", usr.NamaPegawai));
						arrayListParameters.Add(new NpgsqlParameter("waktu", DateTime.Now));
						parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql, parameters);


						tc.Commit();
						tr.Status = true;
						tr.Pesan = "Status Penyerahan Arsip Berhasil diubah";
					}
					catch (Exception ex)
					{
						tc.Rollback();
						tr.Pesan = ex.Message.ToString();
					}
				}
			}
			return tr;
		}

		public TransactionResult batalkanNomor(userIdentity usr, string penomoranid, HttpPostedFileBase fileUploadStream = null)
		{
			var tr = new TransactionResult() { Status = false, Pesan = "Gagal Membatalkan Nomor Surat" };
			string skema = OtorisasiUser.NamaSkema;
			var arrayListParameters = new ArrayList();
			DateTime inputDate = DateTime.Now;

			arrayListParameters.Add(new NpgsqlParameter("penomoranid", penomoranid));
			var parameters = arrayListParameters.OfType<object>().ToArray();
			using (var ctx = new PostgresDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{  
						bool uploaded = true;
						if (fileUploadStream != null)
						{
							uploaded = false;
							int versi = 0;
							string id = penomoranid;

							Stream stream = fileUploadStream.InputStream;

							var reqmessage = new HttpRequestMessage();
							var content = new MultipartFormDataContent();

							content.Add(new StringContent(usr.KantorId), "kantorId");
							content.Add(new StringContent("BerkasPenomoran"), "tipeDokumen");
							content.Add(new StringContent(id), "dokumenId");
							content.Add(new StringContent(".pdf"), "fileExtension");
							content.Add(new StringContent(versi.ToString()), "versionNumber");
							content.Add(new StreamContent(stream), "file", $"SuratPernyataan_{id}");

							reqmessage.Method = HttpMethod.Post;
							reqmessage.Content = content;
							reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

							using (var client = new HttpClient())
							{
								var reqresult = client.SendAsync(reqmessage).Result;
								if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
								{
									// update penomoran
									string sql = $@"UPDATE ""{skema}"".TBLPENOMORAN
										SET STATUSBATAL = '1' WHERE PENOMORANID = :penomoranid";
									arrayListParameters.Add(new NpgsqlParameter("penomoranId", penomoranid));
									parameters = arrayListParameters.OfType<object>().ToArray();
									ctx.Database.ExecuteSqlCommand(sql, parameters);

									//input keterangan file
									sql = $@"INSERT INTO ""{skema}"".TBLKETPENOMORAN (PENOMORANID, LABEL, KETVALUE, WAKTU)
										 VALUES (:penomoranId, :label, :ketvalue, :waktu)";
									arrayListParameters = new ArrayList();
									arrayListParameters.Add(new NpgsqlParameter("penomoranId", penomoranid));
									arrayListParameters.Add(new NpgsqlParameter("label", "FileUploadBatal"));
									arrayListParameters.Add(new NpgsqlParameter("ketvalue", $"SuratPernyataan_{id}"));
									arrayListParameters.Add(new NpgsqlParameter("waktu", inputDate));
									parameters = arrayListParameters.OfType<object>().ToArray();
									ctx.Database.ExecuteSqlCommand(sql, parameters);
									uploaded = true;


								}
								else
								{
									uploaded = false;
								}
							}
						}

						//if (uploaded)
						//{
						//	tc.Commit();
						//	tr.Status = true;
						//	tr.Pesan = "Data Berhasil Disimpan"; 
						//	tr.ReturnValue = penomoranid;
						//}
						//else
						//{
						//	tc.Rollback();
						//	tr.Status = false;
						//	tr.Pesan = "Upload File PDF gagal";
						//	tr.ReturnValue = ""; 
						//}

						tc.Commit();
						tr.Status = true;
						tr.Pesan = "Nomor Surat telah dibatalkan";
					}
					catch (Exception ex)
					{
						tc.Rollback();
						tr.Pesan = ex.Message.ToString();
					}
				}
			}
			return tr;
		}


		public DataPenomoran getPenomoranById(string penomoranid)
		{
			string skema = OtorisasiUser.NamaSkema;
			var result = new DataPenomoran();
			var arrayListParameters = new ArrayList();
			if (!string.IsNullOrEmpty(penomoranid))
			{
				using (var ctx = new PostgresDbContext())
				{
					var sql = $@"SELECT TO_CHAR(TANGGALSURAT,'DD/MM/YYYY') AS TANGGALSURAT, BUKUNOMORID, JENISNASKAHDINAS, NOMORSURAT, NOMORURUT
									FROM ""{skema}"".TBLPENOMORAN 
									WHERE PENOMORANID = :penomoranid";
					arrayListParameters.Add(new NpgsqlParameter("penomoranid", penomoranid));

					var parameters = arrayListParameters.OfType<object>().ToArray();
					result = ctx.Database.SqlQuery<DataPenomoran>(sql, parameters).First();
				}
			}
			return result;
		}

		public TransactionResult penghapusanNomor(userIdentity usr, string penomoranid)
		{
			var tr = new TransactionResult() { Status = false, Pesan = "Gagal Menghapus Nomor Surat" };
			var arrayListParameters = new ArrayList();
			var parameters = new object[0];
			string skema = OtorisasiUser.NamaSkema;
			string suratNum = string.Empty;
			string sql = string.Empty;
			bool createCounter = false;
			bool updateCounter = false;
			decimal? thisnomor = 0;
			decimal? thisnomorlama = 0;
			DateTime inputDate = DateTime.Now;


			using (var ctx = new PostgresDbContext())
			{

				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{

						var dtNomor = getPenomoranById(penomoranid);
						var tahun = dtNomor.TanggalSurat.Split('/')[2];
						var jNaskah = Uri.UnescapeDataString(dtNomor.JenisNaskahDinas + "%20Hapus");

						// cek counter hapus
						decimal? nomorakhir = GetNomorTerakhirPenomoran(dtNomor.BukuNomorId, jNaskah, tahun);
						if (nomorakhir == null) { thisnomor = 1; createCounter = true; }
						else { thisnomor = nomorakhir + 1; updateCounter = true; }
						suratNum = thisnomor.ToString();

						// cek counter sebelumnya
						decimal? nomorakhirlama = GetNomorTerakhirPenomoran(dtNomor.BukuNomorId, Uri.UnescapeDataString(dtNomor.JenisNaskahDinas), tahun);
						if (nomorakhirlama != null) { thisnomorlama = nomorakhirlama - 1; updateCounter = true; }
						else { updateCounter = false; }
						suratNum = thisnomorlama.ToString();

						if (nomorakhirlama == dtNomor.NomorUrut)
						{
							sql = $@"UPDATE ""{skema}"".TBLPENOMORAN
									SET JENISNASKAHDINAS = :jNaskah WHERE PENOMORANID = :penomoranid";
							arrayListParameters = new ArrayList();
							arrayListParameters.Add(new NpgsqlParameter("jNaskah", Uri.EscapeDataString(jNaskah)));
							arrayListParameters.Add(new NpgsqlParameter("penomoranid", penomoranid));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);

							// cek user hapus
							sql = $@"SELECT COUNT(*)
								FROM ""{skema}"".TBLKETPENOMORAN
								WHERE PENOMORANID = :penomoranId AND LABEL = :label";
							arrayListParameters = new ArrayList();
							arrayListParameters.Add(new NpgsqlParameter("penomoranId", penomoranid));
							arrayListParameters.Add(new NpgsqlParameter("label", "UserHapus")); 
							parameters = arrayListParameters.OfType<object>().ToArray();
							var cekuserhapus = ctx.Database.SqlQuery<decimal?>(sql, parameters).FirstOrDefault();

							if (cekuserhapus < 1)
                            {
								//insert ketPenomoran
								sql = $@"INSERT INTO ""{skema}"".TBLKETPENOMORAN (PENOMORANID, LABEL, KETVALUE, WAKTU)
												 VALUES (:penomoranId, :label, :ketvalue, :waktu)";
								//insert UserHapus
								arrayListParameters = new ArrayList();
								arrayListParameters.Add(new NpgsqlParameter("penomoranId", penomoranid));
								arrayListParameters.Add(new NpgsqlParameter("label", "UserHapus"));
								arrayListParameters.Add(new NpgsqlParameter("ketvalue", usr.PegawaiId));
								arrayListParameters.Add(new NpgsqlParameter("waktu", inputDate));
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql, parameters);

							} else
                            {
								//insert ketPenomoran
								sql = $@"UPDATE ""{skema}"".TBLKETPENOMORAN
												SET KETVALUE = :ketvalue, WAKTU = :waktu
												WHERE PENOMORANID = :penomoranid AND LABEL = :label";

								//insert UserHapus
								arrayListParameters = new ArrayList();
								arrayListParameters.Add(new NpgsqlParameter("penomoranId", penomoranid));
								arrayListParameters.Add(new NpgsqlParameter("label", "UserHapus"));
								arrayListParameters.Add(new NpgsqlParameter("ketvalue", usr.PegawaiId));
								arrayListParameters.Add(new NpgsqlParameter("waktu", inputDate));
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql, parameters);
							} 
							 
							if (createCounter)
							{
								arrayListParameters = new ArrayList();
								sql = $@"INSERT INTO ""{skema}"".TBLCOUNTERPENOMORAN (BUKUNOMORID, JENISNASKAHDINAS, COUNTER, TAHUNBERJALAN) 
											 VALUES (:bkid, :jenisnaskah, :count, :tahun)";
								arrayListParameters.Add(new NpgsqlParameter("bkid", dtNomor.BukuNomorId));
								arrayListParameters.Add(new NpgsqlParameter("jenisnaskah", Uri.EscapeDataString(jNaskah)));
								arrayListParameters.Add(new NpgsqlParameter("count", 1));
								arrayListParameters.Add(new NpgsqlParameter("tahun", int.Parse(tahun)));
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql, parameters); 

							}
							else if (updateCounter)
							{
								arrayListParameters = new ArrayList();
								sql = $@"UPDATE ""{skema}"".TBLCOUNTERPENOMORAN 
											 SET COUNTER = :count
											 WHERE BUKUNOMORID = :bkid AND JENISNASKAHDINAS = :jenisnaskah AND TAHUNBERJALAN = :tahun ";
								arrayListParameters.Add(new NpgsqlParameter("count", thisnomor));
								arrayListParameters.Add(new NpgsqlParameter("bkid", dtNomor.BukuNomorId));
								arrayListParameters.Add(new NpgsqlParameter("jenisnaskah", Uri.EscapeDataString(jNaskah)));
								arrayListParameters.Add(new NpgsqlParameter("tahun", int.Parse(tahun)));
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql, parameters);

								arrayListParameters = new ArrayList();
								sql = $@"UPDATE ""{skema}"".TBLCOUNTERPENOMORAN 
												 SET COUNTER = :count
												 WHERE BUKUNOMORID = :bkid AND JENISNASKAHDINAS = :jenisnaskah AND TAHUNBERJALAN = :tahun ";
								arrayListParameters.Add(new NpgsqlParameter("count", thisnomorlama));
								arrayListParameters.Add(new NpgsqlParameter("bkid", dtNomor.BukuNomorId));
								arrayListParameters.Add(new NpgsqlParameter("jenisnaskah", dtNomor.JenisNaskahDinas));
								arrayListParameters.Add(new NpgsqlParameter("tahun", int.Parse(tahun)));
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql, parameters);

								
							}


							tc.Commit();
							tr.Status = true;
							tr.Pesan = "Nomor Surat telah dihapus";
						}


					}
					catch (Exception ex)
					{
						tc.Rollback();
						tr.Pesan = ex.Message.ToString();
					}
				}

			}

			return tr;
		}

		public TransactionResult restoreNomor(string penomoranid)
		{
			var tr = new TransactionResult() { Status = false, Pesan = "Gagal Mengembalikan Nomor Surat" };
			string skema = OtorisasiUser.NamaSkema;
			string suratNum = string.Empty;
			string suratNumLama = string.Empty;
			var nomorsurat = string.Empty;
			var nomorsuratbaru = string.Empty;
			bool createCounter = false;
			bool updateCounter = false;
			decimal? thisnomor = 0;
			decimal? thisnomorlama = 0;
			string sql = string.Empty;
			string kode = string.Empty;
			string profilePenandatangan = string.Empty;
			string klasArsip = string.Empty;
			var arrayListParameters = new ArrayList();
			var parameters = new object[0];

			using (var ctx = new PostgresDbContext())
			{

				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{


						var dtNomor = getPenomoranById(penomoranid);
						var tahun = dtNomor.TanggalSurat.Split('/')[2];
						var jNaskah = Uri.UnescapeDataString(dtNomor.JenisNaskahDinas + "%20Hapus");

						if (jNaskah.Contains(" Hapus"))
						{
							jNaskah = jNaskah.Replace(" Hapus", "");
						}

						// cek counter baru
						decimal? nomorakhir = GetNomorTerakhirPenomoran(dtNomor.BukuNomorId, jNaskah, tahun);
						if (nomorakhir == null) { thisnomor = 1; createCounter = true; }
						else { thisnomor = nomorakhir + 1; updateCounter = true; }
						suratNum = thisnomor.ToString();

						// cek counter hapus
						decimal? nomorakhirlama = GetNomorTerakhirPenomoran(dtNomor.BukuNomorId, Uri.UnescapeDataString(dtNomor.JenisNaskahDinas), tahun);
						if (nomorakhirlama != null) { thisnomorlama = nomorakhirlama - 1; updateCounter = true; }
						else { updateCounter = false; }
						suratNumLama = thisnomorlama.ToString();


						var dtDetail = GetDetailsPenomoran(penomoranid);


						//get kode penandatangan 
						var kodePenandatangan = getListJabatan(ProfileId: dtDetail[2].ValueKeterangan);
						kode = (kodePenandatangan.Count > 0) ? kodePenandatangan[0].KodeTTD : string.Empty;

						//get nomorsurat full
						if (dtDetail[2].ValueKeterangan.Equals("H0000001") || dtDetail[2].ValueKeterangan.Equals("H0000002")) { dtNomor.ismenteri = true; }
						nomorsurat = PenomoranSuratBuilder(suratNum, kode, dtDetail[3].ValueKeterangan, jNaskah, dtNomor.TanggalSurat, ismenteri: dtNomor.ismenteri);


						sql = $@"UPDATE ""{skema}"".TBLPENOMORAN
								SET JENISNASKAHDINAS = :jNaskah, NOMORURUT = :thisnomor, NOMORSURAT = :nomorsurat WHERE PENOMORANID = :penomoranid";
						arrayListParameters.Add(new NpgsqlParameter("thisnomor", thisnomor));
						arrayListParameters.Add(new NpgsqlParameter("nomorsurat", Uri.EscapeDataString(nomorsurat)));
						arrayListParameters.Add(new NpgsqlParameter("jNaskah", Uri.EscapeDataString(jNaskah)));
						arrayListParameters.Add(new NpgsqlParameter("penomoranid", penomoranid));
						parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql, parameters);

						if (createCounter)
						{
							arrayListParameters = new ArrayList();
							sql = $@"INSERT INTO ""{skema}"".TBLCOUNTERPENOMORAN (BUKUNOMORID, JENISNASKAHDINAS, COUNTER, TAHUNBERJALAN) 
										 VALUES (:bkid, :jenisnaskah, :count, :tahun)";
							arrayListParameters.Add(new NpgsqlParameter("bkid", dtNomor.BukuNomorId));
							arrayListParameters.Add(new NpgsqlParameter("jenisnaskah", Uri.EscapeDataString(jNaskah)));
							arrayListParameters.Add(new NpgsqlParameter("count", 1));
							arrayListParameters.Add(new NpgsqlParameter("tahun", int.Parse(tahun)));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);

						}
						else if (updateCounter)
						{

							arrayListParameters = new ArrayList();
							sql = $@"UPDATE ""{skema}"".TBLCOUNTERPENOMORAN 
										 SET COUNTER = :count
										 WHERE BUKUNOMORID = :bkid AND JENISNASKAHDINAS = :jenisnaskah AND TAHUNBERJALAN = :tahun ";
							arrayListParameters.Add(new NpgsqlParameter("count", thisnomor));
							arrayListParameters.Add(new NpgsqlParameter("bkid", dtNomor.BukuNomorId));
							arrayListParameters.Add(new NpgsqlParameter("jenisnaskah", Uri.EscapeDataString(jNaskah)));
							arrayListParameters.Add(new NpgsqlParameter("tahun", int.Parse(tahun)));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);

							arrayListParameters = new ArrayList();
							sql = $@"UPDATE ""{skema}"".TBLCOUNTERPENOMORAN 
											 SET COUNTER = :count
											 WHERE BUKUNOMORID = :bkid AND JENISNASKAHDINAS = :jenisnaskah AND TAHUNBERJALAN = :tahun ";
							arrayListParameters.Add(new NpgsqlParameter("count", thisnomorlama));
							arrayListParameters.Add(new NpgsqlParameter("bkid", dtNomor.BukuNomorId));
							arrayListParameters.Add(new NpgsqlParameter("jenisnaskah", dtNomor.JenisNaskahDinas));
							arrayListParameters.Add(new NpgsqlParameter("tahun", int.Parse(tahun)));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);
						}


						tc.Commit();
						tr.Status = true;
						tr.Pesan = "Nomor Surat telah dipulihkan dengan nomor baru";
					}
					catch (Exception ex)
					{
						tc.Rollback();
						tr.Pesan = ex.Message.ToString();
					}
				}

			}

			return tr;
		}

		public bool validasiUserBukuPenomoran(string bukunomorid, string pegawaiid)
		{
			string skema = OtorisasiUser.NamaSkema;
			var arrayListParameters = new ArrayList();
			var parameters = arrayListParameters.OfType<object>().ToArray();
			string sql = $@"SELECT
								COUNT(PEGAWAIID)
							FROM
								""{skema}"".TBLAKSESBUKU
							WHERE
								BUKUNOMORID = :bukunomorid 
								AND STATUSAKTIF <> '0' 
								AND PEGAWAIID = :pegawaiid";
			arrayListParameters.Add(new NpgsqlParameter("bukunomorid", bukunomorid));
			arrayListParameters.Add(new NpgsqlParameter("pegawaiid", pegawaiid));
			parameters = arrayListParameters.OfType<object>().ToArray();
			var ctx = new PostgresDbContext();
			bool check = ctx.Database.SqlQuery<decimal>(sql, parameters).FirstOrDefault() > 0;
			return check;
		}

		public List<TipeSurat> getTipeSurat(string tipe = null)
		{
			var datas = new List<TipeSurat>();
			string skema = OtorisasiUser.NamaSkema;
			var arrayListParameters = new ArrayList();
			string sql = $@"SELECT 
								JENISNASKAH AS NAMATIPESURAT,
								KODENASKAH AS KODETIPESURAT,
								TAMPIL AS VALUETIPESURAT,
								FORMATNOMOR,
								KEWENANGAN
							FROM ""{skema}"".JENISNASKAHDINAS
							WHERE FUNGSI = 'naskahdinas' AND STATUSHAPUS = '0'
						";
			if (!string.IsNullOrEmpty(tipe))
			{
				sql += " AND JENISNASKAH = :tipe ";
				arrayListParameters.Add(new NpgsqlParameter("tipe", tipe));
			}

			sql += " ORDER BY URUT, JENISNASKAH ";
			using (var ctx = new PostgresDbContext())
			{
				var parameters = arrayListParameters.OfType<object>().ToArray();
				datas = ctx.Database.SqlQuery<TipeSurat>(sql, parameters).ToList();
				foreach (var d in datas)
				{
					d.FormatNomor = Uri.UnescapeDataString(d.FormatNomor);
				}
			}

			return datas;
		}

		public List<DataPenomoran> getDataPenomoran(
			string bukunomorid,
			string jenis,
			string tahun,
			int? from = 0,
			int? to = 0,
			string searchKey = null,
			string Bulan = null,
			string StatArsip = null
			)
		{
			string skema = OtorisasiUser.NamaSkema;
			var datas = new List<DataPenomoran>();
			var arrayListParameters = new ArrayList();
			if (!string.IsNullOrEmpty(bukunomorid ?? jenis ?? tahun))
			{
				string sql = $@"SELECT ROW_NUMBER() OVER( ORDER BY TP.NOMORURUT DESC, TP.BACKDATE DESC ) RNUMBER,
									   COUNT(1) OVER() AS TOTAL,
									   TP.PENOMORANID,
									   TO_CHAR(TP.TANGGALINPUT,'DD/MM/YYYY HH24:MI') AS TANGGALINPUT,
									   TP.NOMORURUT,
									   TO_CHAR(TP.NOMORURUT, 'FM9999') || CASE WHEN TP.BACKDATE > 0 THEN '.' || TO_CHAR(TP.BACKDATE,'FM9999') ELSE '' END AS SNOMORURUT,
									   TO_CHAR(TP.TANGGALSURAT,'DD/MM/YYYY') AS TANGGALSURAT,
									   TP.NOMORSURAT,
									   TP.PERIHAL,
									   TP.JENISNASKAHDINAS,
									   TP.BUKUNOMORID,
									   TP.STATUS,
									   TP.KETERANGAN,
									   TP.STATUSBATAL,
									   TK.KETVALUE AS PROFILEPENANDATANGAN,
									   TK1.KETVALUE AS KLASIFIKASIARSIP,
									   TP.BACKDATE AS SISIP
									FROM
									   ""{skema}"".TBLPENOMORAN TP
									LEFT JOIN ""{skema}"".TBLKETPENOMORAN TK
									   ON TP.PENOMORANID = TK.PENOMORANID AND TK.LABEL = 'penandatangan'
									LEFT JOIN ""{skema}"".TBLKETPENOMORAN TK1
									   ON TP.PENOMORANID = TK1.PENOMORANID AND TK1.LABEL = 'klasArsip'
									WHERE
									   TP.BUKUNOMORID = :bukunomorid AND TP.JENISNASKAHDINAS = :jenis AND EXTRACT(YEAR FROM TP.TANGGALSURAT) = :tahun 
									   AND STATUS != 'BOOK' ";
				arrayListParameters.Add(new NpgsqlParameter("bukunomorid", bukunomorid));
				arrayListParameters.Add(new NpgsqlParameter("jenis", jenis));
				arrayListParameters.Add(new NpgsqlParameter("tahun", int.Parse(tahun)));

				if (!string.IsNullOrEmpty(searchKey))
				{
					sql += " AND UPPER(NOMORSURAT) || UPPER(PERIHAL) || UPPER(TO_CHAR(TANGGALSURAT,'DD/MM/YYYY')) LIKE '%'||UPPER(:searchKey)||'%'  ";
					arrayListParameters.Add(new NpgsqlParameter("searchKey", searchKey));
				}

				if (!string.IsNullOrEmpty(Bulan))
				{
					sql += " AND TO_CHAR(EXTRACT(MONTH FROM TP.TANGGALSURAT),'FM9999') = :bulan ";
					arrayListParameters.Add(new NpgsqlParameter("bulan", Bulan));
				}

				if (!string.IsNullOrEmpty(StatArsip))
				{
					if (StatArsip == "TTE")
					{
						sql += " AND KETERANGAN = :tte ";
						arrayListParameters.Add(new NpgsqlParameter("tte", StatArsip));
					}
					else if (StatArsip == "Batal")
					{
						sql += " AND STATUSBATAL = :batal ";
						arrayListParameters.Add(new NpgsqlParameter("batal", "1"));
					}
					else
					{
						sql += " AND STATUS = :stat AND KETERANGAN = 'Manual'";
						arrayListParameters.Add(new NpgsqlParameter("stat", StatArsip));
					}

				}

				sql += " ORDER BY TP.NOMORURUT DESC, TP.BACKDATE DESC ";

				if (to > 0)
				{
					sql = $"SELECT DATA.* FROM ({sql}) AS DATA WHERE DATA.RNUMBER BETWEEN :frmthis AND :tothis";
					arrayListParameters.Add(new NpgsqlParameter("frmthis", from));
					arrayListParameters.Add(new NpgsqlParameter("tothis", to));
				}

				using (var ctx = new PostgresDbContext())
				{
					var parameters = arrayListParameters.OfType<object>().ToArray();
					datas = ctx.Database.SqlQuery<DataPenomoran>(sql, parameters).ToList();
				}

			}
			return datas;
		}

		public string getDataPenomoranAkhir(string bukunomorid, string jenis)
		{
			string skema = OtorisasiUser.NamaSkema;
			string result = "";
			var arrayListParameters = new ArrayList();
			if (!string.IsNullOrEmpty(bukunomorid ?? jenis))
			{
				using (var ctx = new PostgresDbContext())
				{
					var sql = $@"SELECT NOMORSURAT
											FROM
											   ""{skema}"".TBLPENOMORAN 
											WHERE
											   BUKUNOMORID = :bukunomorid AND JENISNASKAHDINAS = :jenis
											   AND STATUS != 'BOOK' ";
					arrayListParameters.Add(new NpgsqlParameter("bukunomorid", bukunomorid));
					arrayListParameters.Add(new NpgsqlParameter("jenis", jenis));
					sql += " ORDER BY NOMORURUT DESC LIMIT 1";

					var parameters = arrayListParameters.OfType<object>().ToArray();
					result = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault<string>();
				}
			}
			return result;
		}


		public List<KlasifikasiArsip> getListKlasArsip(string kode = null, int? from = 0, int? to = 0, string searchKey = null)
		{
			var datas = new List<KlasifikasiArsip>();
			var arrayListParameters = new ArrayList();
			string sql = $@"SELECT ROW_NUMBER() OVER(ORDER BY KODEKLASIFIKASI) RNUMBER,
								   COUNT(1) OVER() AS TOTAL,
								   KODEKLASIFIKASI,
								   JENISARSIP,
								   KETERANGAN
							FROM SURAT.KLASIFIKASIARSIP
							WHERE STATUSHAPUS = 0 ";

			if (!string.IsNullOrEmpty(kode))
			{
				sql += " AND KODEKLASIFIKASI = :kode ";
				arrayListParameters.Add(new OracleParameter("kode", kode));
			}

			if (!string.IsNullOrEmpty(searchKey))
			{
				sql += " AND UPPER(APEX_UTIL.URL_ENCODE(KODEKLASIFIKASI)) || UPPER(APEX_UTIL.URL_ENCODE(JENISARSIP)) LIKE '%'||UPPER(:searchKey)||'%' ";
				arrayListParameters.Add(new OracleParameter("searchKey", searchKey));
			}

			sql += " ORDER BY KODEKLASIFIKASI ";

			if (to > 0)
			{
				sql = $"SELECT DATA.* FROM ({sql}) DATA WHERE DATA.RNUMBER BETWEEN :frmthis AND :tothis";
				arrayListParameters.Add(new OracleParameter("frmthis", from));
				arrayListParameters.Add(new OracleParameter("tothis", to));
			}

			using (var ctx = new BpnDbContext())
			{
				var parameters = arrayListParameters.OfType<object>().ToArray();
				datas = ctx.Database.SqlQuery<KlasifikasiArsip>(sql, parameters).ToList();
			}

			return datas;
		}

		public TransactionResult simpanKodeTtd(string profileid, string eselon, string kodettd, string KodeTTDSakter)
		{
			TransactionResult tr = new TransactionResult() { Status = false, Pesan = "data gagal diubah" };
			using (var ctx = new BpnDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{
						string kodetandatangan = "0";
						if (kodettd != KodeTTDSakter)
						{
							if (kodettd.Contains(KodeTTDSakter) && kodettd.Length > KodeTTDSakter.Length)
							{
								kodetandatangan = kodettd.Replace(KodeTTDSakter + ".", "");
							}
						}

						var arrayListParameters = new ArrayList();
						string sql = $@"UPDATE JABATAN
										SET TIPEESELONID = :eselon, KODETTD = :kodettd
										WHERE PROFILEID = :profileid";
						arrayListParameters.Add(new OracleParameter("eselon", int.Parse(eselon)));
						arrayListParameters.Add(new OracleParameter("kodettd", kodetandatangan));
						arrayListParameters.Add(new OracleParameter("profileid", profileid));
						var parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql, parameters);
						tc.Commit();
						tr.Status = true;
						tr.Pesan = "Data Berhasil Disimpan";
					}
					catch (Exception ex)
					{
						tc.Rollback();
						tr.Pesan = ex.Message.ToString();
					}
					finally
					{
						tc.Dispose();
						ctx.Dispose();
					}
				}
			}
			return tr;
		}

		public List<string> getListTahun(string bukunomorid)
		{
			string skema = OtorisasiUser.NamaSkema;
			var datas = new List<string>();
			var arrayListParameters = new ArrayList();
			if (!string.IsNullOrEmpty(bukunomorid))
			{
				string sql = $@"SELECT 
									TO_CHAR(EXTRACT(YEAR FROM TANGGALSURAT),'FM9999') 
								FROM ""{skema}"".tblpenomoran 
								WHERE BUKUNOMORID = :bukunomorid AND (STATUSBATAL = '0' OR STATUSBATAL IS NULL) AND STATUS != 'BOOK'
								GROUP BY EXTRACT(YEAR FROM TANGGALSURAT) ORDER BY EXTRACT(YEAR FROM TANGGALSURAT) DESC";

				arrayListParameters.Add(new NpgsqlParameter("bukunomorid", bukunomorid));
				using (var ctx = new PostgresDbContext())
				{
					var parameters = arrayListParameters.OfType<object>().ToArray();
					datas = ctx.Database.SqlQuery<string>(sql, parameters).ToList();
				}
			}

			return datas;
		}

		public string NewUUID()
		{
			string _result = "";
			using (var ctx = new PostgresDbContext())
			{
				_result = ctx.Database.SqlQuery<string>("SELECT sysuuid()").FirstOrDefault<string>();
			}

			return _result;
		}

		public TransactionResult SimpanDataPenomoran(userIdentity usr, DataPenomoran data, HttpPostedFileBase fileUploadStream = null)
		{
			string skema = OtorisasiUser.NamaSkema;
			var tr = new TransactionResult() { Status = false, Pesan = "Data gagal disimpan" };
			using (var ctx = new PostgresDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{
						string sql = string.Empty;
						var arrayListParameters = new ArrayList();
						var parameters = new object[0];

						decimal? thisnomor = 0;
						decimal? backdate = 0;
						string suratNum = string.Empty;
						string nomorsurat = string.Empty;
						string kode = string.Empty;
						var tahun = data.TanggalSurat.Split('/')[2];
						string statusin = data.Status;
						var penomoranId = NewUUID();
						DateTime inputDate = DateTime.Now;
						string mssgadd = "";
						bool createCounter = false;
						bool updateCounter = false;
						var namasatker = new DataMasterModel().GetNamaUnitKerjaById(usr.UnitKerjaId);

						//get nomorurut dengan kondisi
						if (data.BackDate)
						{
							//nomor mundur - cari sub nomornya
							var bdNum = GetNomorOnDate(data.BukuNomorId, data.JenisNaskahDinas, data.TanggalSurat, data.NomorUrut);
							if (bdNum == null)
							{
								backdate = 1;
							}
							else
							{
								backdate = bdNum + 1;
							}
							suratNum = data.NomorUrut.ToString() + "." + backdate.ToString();
							thisnomor = data.NomorUrut;
						}
						else if (data.Book)
						{
							//kosongin nomornya dulu
							suratNum = "<BOOK>";
							statusin = "BOOK";
						}
						else
						{
							//cek ada bookingan atau tidak
							if (!BookedDate(data.BukuNomorId))
							{
								mssgadd = " - Catatan : Booking Nomor Mengalamai Kendala";
							}

							//nomor biasa - cari nomor akhir
							decimal? nomorakhir = GetNomorTerakhirPenomoran(data.BukuNomorId, data.JenisNaskahDinas, tahun);
							if (nomorakhir == null) { thisnomor = 1; createCounter = true; }
							else { thisnomor = nomorakhir + 1; updateCounter = true; }
							suratNum = thisnomor.ToString();
						}

						//get kode penandatangan
						var kodePenandatangan = getListJabatan(ProfileId: data.ProfilePenandatangan);
						kode = (kodePenandatangan.Count > 0) ? kodePenandatangan[0].KodeTTD : string.Empty;

						//get nomorsurat full
						if (data.ProfilePenandatangan.Equals("H0000001") || data.ProfilePenandatangan.Equals("H0000002")) { data.ismenteri = true; }
						nomorsurat = PenomoranSuratBuilder(suratNum, kode, data.KlasifikasiArsip, data.JenisNaskahDinas, data.TanggalSurat, ismenteri: data.ismenteri);

						sql = $@"INSERT INTO ""{skema}"".TBLPENOMORAN 
								(PENOMORANID, TANGGALINPUT, NOMORURUT, TANGGALSURAT, NOMORSURAT, PERIHAL, JENISNASKAHDINAS, BUKUNOMORID, STATUS, KETERANGAN, BACKDATE, STATUSBATAL)
								VALUES (:penomoranId, :tanggalinput, :thisnomor, TO_DATE(:tanggalsurat,'DD/MM/YYYY'), :nomorsurat, :perihal, :jenis, :bukunomorid, :status, :keterangan, :backdate, :statusbatal)";
						arrayListParameters.Add(new NpgsqlParameter("penomoranId", penomoranId));
						arrayListParameters.Add(new NpgsqlParameter("tanggalinput", inputDate));
						arrayListParameters.Add(new NpgsqlParameter("thisnomor", thisnomor));
						arrayListParameters.Add(new NpgsqlParameter("tanggalsurat", data.TanggalSurat));
						arrayListParameters.Add(new NpgsqlParameter("nomorsurat", Uri.EscapeDataString(nomorsurat)));
						arrayListParameters.Add(new NpgsqlParameter("perihal", Uri.EscapeDataString(data.Perihal)));
						arrayListParameters.Add(new NpgsqlParameter("jenis", Uri.EscapeDataString(data.JenisNaskahDinas)));
						arrayListParameters.Add(new NpgsqlParameter("bukunomorid", data.BukuNomorId));
						arrayListParameters.Add(new NpgsqlParameter("status", statusin));
						arrayListParameters.Add(new NpgsqlParameter("keterangan", data.Keterangan));
						arrayListParameters.Add(new NpgsqlParameter("backdate", backdate));
						arrayListParameters.Add(new NpgsqlParameter("statusbatal", "0"));
						parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql, parameters);

						//insert ketPenomoran
						sql = $@"INSERT INTO ""{skema}"".TBLKETPENOMORAN (PENOMORANID, LABEL, KETVALUE, WAKTU)
										 VALUES (:penomoranId, :label, :ketvalue, :waktu)";

						//insert inputUser
						arrayListParameters = new ArrayList();
						arrayListParameters.Add(new NpgsqlParameter("penomoranId", penomoranId));
						arrayListParameters.Add(new NpgsqlParameter("label", "UserInput"));
						arrayListParameters.Add(new NpgsqlParameter("ketvalue", usr.PegawaiId));
						arrayListParameters.Add(new NpgsqlParameter("waktu", inputDate));
						parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql, parameters);

						//insert satkerInput
						arrayListParameters = new ArrayList();
						arrayListParameters.Add(new NpgsqlParameter("penomoranId", penomoranId));
						arrayListParameters.Add(new NpgsqlParameter("label", "satkerInput"));
						arrayListParameters.Add(new NpgsqlParameter("ketvalue", namasatker));
						arrayListParameters.Add(new NpgsqlParameter("waktu", inputDate));
						parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql, parameters);

						//insert penandatangan
						arrayListParameters = new ArrayList();
						arrayListParameters.Add(new NpgsqlParameter("penomoranId", penomoranId));
						arrayListParameters.Add(new NpgsqlParameter("label", "penandatangan"));
						arrayListParameters.Add(new NpgsqlParameter("ketvalue", data.ProfilePenandatangan));
						arrayListParameters.Add(new NpgsqlParameter("waktu", inputDate));
						parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql, parameters);

						//insert klas arsip
						arrayListParameters = new ArrayList();
						arrayListParameters.Add(new NpgsqlParameter("penomoranId", penomoranId));
						arrayListParameters.Add(new NpgsqlParameter("label", "klasArsip"));
						arrayListParameters.Add(new NpgsqlParameter("ketvalue", data.KlasifikasiArsip));
						arrayListParameters.Add(new NpgsqlParameter("waktu", inputDate));
						parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql, parameters);

						if (!string.IsNullOrEmpty(data.Pemohon))
                        {
							//insert pemohon sisip
							arrayListParameters = new ArrayList();
							arrayListParameters.Add(new NpgsqlParameter("penomoranId", penomoranId));
							arrayListParameters.Add(new NpgsqlParameter("label", "Pemohon"));
							arrayListParameters.Add(new NpgsqlParameter("ketvalue", data.Pemohon));
							arrayListParameters.Add(new NpgsqlParameter("waktu", inputDate));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);
						}

						//jika arsip sudah, simpan penerima arsip
						if (data.Status == "1")
						{
							arrayListParameters = new ArrayList();
							arrayListParameters.Add(new NpgsqlParameter("penomoranId", penomoranId));
							arrayListParameters.Add(new NpgsqlParameter("label", "PICARSIP"));
							arrayListParameters.Add(new NpgsqlParameter("ketvalue", usr.PegawaiId));
							arrayListParameters.Add(new NpgsqlParameter("waktu", inputDate));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);
						}

						if (!string.IsNullOrEmpty(data.details))
						{
							foreach (var detail in data.details.Split('^'))
							{
								arrayListParameters = new ArrayList();
								var thisdetail = detail.Split('|');
								arrayListParameters.Add(new NpgsqlParameter("penomoranId", penomoranId));
								arrayListParameters.Add(new NpgsqlParameter("label", thisdetail[0]));
								arrayListParameters.Add(new NpgsqlParameter("ketvalue", thisdetail[1]));
								arrayListParameters.Add(new NpgsqlParameter("waktu", inputDate));
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql, parameters);
							}
						}

						if (createCounter)
						{
							arrayListParameters = new ArrayList();
							sql = $@"INSERT INTO ""{skema}"".TBLCOUNTERPENOMORAN (BUKUNOMORID, JENISNASKAHDINAS, COUNTER, TAHUNBERJALAN) 
									 VALUES (:bkid, :jenisnaskah, :count, :tahun)";
							arrayListParameters.Add(new NpgsqlParameter("bkid", data.BukuNomorId));
							arrayListParameters.Add(new NpgsqlParameter("jenisnaskah", Uri.EscapeDataString(data.JenisNaskahDinas)));
							arrayListParameters.Add(new NpgsqlParameter("count", 1));
							arrayListParameters.Add(new NpgsqlParameter("tahun", int.Parse(tahun)));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);
						}
						else if (updateCounter)
						{
							arrayListParameters = new ArrayList();
							sql = $@"UPDATE ""{skema}"".TBLCOUNTERPENOMORAN 
									 SET COUNTER = :count
									 WHERE BUKUNOMORID = :bkid AND JENISNASKAHDINAS = :jenisnaskah AND TAHUNBERJALAN = :tahun ";
							arrayListParameters.Add(new NpgsqlParameter("count", thisnomor));
							arrayListParameters.Add(new NpgsqlParameter("bkid", data.BukuNomorId));
							arrayListParameters.Add(new NpgsqlParameter("jenisnaskah", Uri.EscapeDataString(data.JenisNaskahDinas)));
							arrayListParameters.Add(new NpgsqlParameter("tahun", int.Parse(tahun)));
							parameters = arrayListParameters.OfType<object>().ToArray();
							ctx.Database.ExecuteSqlCommand(sql, parameters);
						}

						bool uploaded = true;
						if (fileUploadStream != null)
						{
							uploaded = false;
							int versi = 0;
							string id = penomoranId;

							Stream stream = fileUploadStream.InputStream;

							var reqmessage = new HttpRequestMessage();
							var content = new MultipartFormDataContent();

							content.Add(new StringContent(usr.KantorId), "kantorId");
							content.Add(new StringContent("BerkasPenomoran"), "tipeDokumen");
							content.Add(new StringContent(id), "dokumenId");
							content.Add(new StringContent(".pdf"), "fileExtension");
							content.Add(new StringContent(versi.ToString()), "versionNumber");
							content.Add(new StreamContent(stream), "file", $"SuratPernyataan_{id}");

							reqmessage.Method = HttpMethod.Post;
							reqmessage.Content = content;
							reqmessage.RequestUri = new Uri(string.Concat(ConfigurationManager.AppSettings["ServiceEofficeUrl"].ToString(), "Store"));

							using (var client = new HttpClient())
							{
								var reqresult = client.SendAsync(reqmessage).Result;
								if (reqresult.IsSuccessStatusCode && reqresult.StatusCode == HttpStatusCode.OK)
								{
									//input keterangan file
									sql = $@"INSERT INTO ""{skema}"".TBLKETPENOMORAN (PENOMORANID, LABEL, KETVALUE, WAKTU)
										 VALUES (:penomoranId, :label, :ketvalue, :waktu)";
									arrayListParameters = new ArrayList();
									arrayListParameters.Add(new NpgsqlParameter("penomoranId", penomoranId));
									arrayListParameters.Add(new NpgsqlParameter("label", "FileUpload"));
									arrayListParameters.Add(new NpgsqlParameter("ketvalue", $"SuratPernyataan_{id}"));
									arrayListParameters.Add(new NpgsqlParameter("waktu", inputDate));
									parameters = arrayListParameters.OfType<object>().ToArray();
									ctx.Database.ExecuteSqlCommand(sql, parameters);
									uploaded = true;


								}
								else
								{
									uploaded = false;
								}
							}
						}

						if (uploaded)
						{
							tc.Commit();
							tr.Status = true;
							tr.Pesan = "Data Berhasil Disimpan" + mssgadd;
							tr.ReturnValue = nomorsurat;
							tr.ReturnValue2 = penomoranId;
						}
						else
						{
							tc.Rollback();
							tr.Status = false;
							tr.Pesan = "Upload File PDF gagal";
							tr.ReturnValue = "";
							tr.ReturnValue2 = "";
						}


					}
					catch (Exception ex)
					{
						tc.Rollback();
						tr.Pesan = ex.Message.ToString();
					}
					finally
					{
						tc.Dispose();
						ctx.Dispose();
					}
				}
			}
			return tr;
		}

		public bool BookedDate(string bkid)
		{
			string skema = OtorisasiUser.NamaSkema;
			var datas = new List<DataBooking>();
			var arrayListParameters = new ArrayList();
			var parameters = arrayListParameters.OfType<object>().ToArray();
			bool status = false;

			//get list book nomor
			string sql = $@"SELECT PENOMORANID, 
								   NOMORURUT, 
								   NOMORSURAT, 
								   TO_CHAR(TANGGALSURAT,'DD/MM/YYYY') AS TANGGALSURAT, 
								   JENISNASKAHDINAS 
							FROM ""{skema}"".TBLPENOMORAN
							WHERE STATUS = 'BOOK' AND (STATUSBATAL = '0' OR STATUSBATAL IS NULL) AND BUKUNOMORID = :bkid";
			arrayListParameters.Add(new NpgsqlParameter("bkid", bkid));
			using (var ctx = new PostgresDbContext())
			{
				try
				{
					parameters = arrayListParameters.OfType<object>().ToArray();
					datas = ctx.Database.SqlQuery<DataBooking>(sql, parameters).ToList();
					status = true;
				}
				catch (Exception e)
				{
					status = false;
				}

			}

			if (datas.Count > 0)
			{
				foreach (var data in datas)
				{
					arrayListParameters = new ArrayList();
					decimal? nomorakhir = GetNomorTerakhirPenomoran(bkid, Uri.UnescapeDataString(data.JenisNaskahDinas), data.TanggalSurat.Split('/')[2]);
					sql = $@"UPDATE ""{skema}"".TBLPENOMORAN
								 SET NOMORSURAT = :nomorsurat, NOMORURUT = :nomorurut, STATUS = '0'
								 WHERE PENOMORANID = :penomoranid AND BUKUNOMORID = :bkid AND STATUS = 'BOOK'";
					arrayListParameters.Add(new NpgsqlParameter("nomorsurat", data.NomorSurat.Replace(Uri.EscapeDataString("<BOOK>"), (nomorakhir + 1).ToString())));
					arrayListParameters.Add(new NpgsqlParameter("nomorurut", nomorakhir + 1));
					arrayListParameters.Add(new NpgsqlParameter("penomoranid", data.Penomoranid));
					arrayListParameters.Add(new NpgsqlParameter("bkid", bkid));
					parameters = arrayListParameters.OfType<object>().ToArray();
					using (var ctx = new PostgresDbContext())
					{
						using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
						{
							try
							{
								ctx.Database.ExecuteSqlCommand(sql, parameters);
								tc.Commit();
							}
							catch (Exception ex)
							{
								tc.Rollback();
								status = false;
							}
							finally
							{
								tc.Dispose();
								ctx.Dispose();
							}
						}
					}
				}
				status = true;
			}
			return status;
		}

		public TransactionResult UpdateDataPenomoran(userIdentity usr, DataPenomoran data)
		{
			string skema = OtorisasiUser.NamaSkema;
			var tr = new TransactionResult() { Status = false, Pesan = "Data gagal diperbaharui" };
			using (var ctx = new PostgresDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{
						string sql = string.Empty;
						var arrayListParameters = new ArrayList();
						var parameters = new object[0];
						sql = $@"UPDATE ""{skema}"".TBLPENOMORAN
								 SET NOMORSURAT = :nomorsurat, PERIHAL = :perihal, STATUS = :status, KETERANGAN = :keterangan, TANGGALSURAT = TO_DATE(:tanggalsurat,'DD/MM/YYYY')
								 WHERE PENOMORANID = :penomoranId";
						arrayListParameters.Add(new NpgsqlParameter("nomorsurat", Uri.EscapeDataString(data.NomorSurat)));
						arrayListParameters.Add(new NpgsqlParameter("perihal", Uri.EscapeDataString(data.Perihal)));
						arrayListParameters.Add(new NpgsqlParameter("tanggalsurat", data.TanggalSurat));
						arrayListParameters.Add(new NpgsqlParameter("status", data.Status));
						arrayListParameters.Add(new NpgsqlParameter("keterangan", data.Keterangan));
						arrayListParameters.Add(new NpgsqlParameter("penomoranId", data.Penomoranid));
						parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql, parameters);

						//Update ketPenomoran
						sql = $@"UPDATE ""{skema}"".TBLKETPENOMORAN SET KETVALUE = :ketvalue WHERE PENOMORANID = :penomoranId AND LABEL = :label";

						//Update penandatangan
						arrayListParameters = new ArrayList();
						arrayListParameters.Add(new NpgsqlParameter("ketvalue", data.ProfilePenandatangan));
						arrayListParameters.Add(new NpgsqlParameter("penomoranId", data.Penomoranid));
						arrayListParameters.Add(new NpgsqlParameter("label", "penandatangan"));
						parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql, parameters);

						//Insert klasArsip
						arrayListParameters = new ArrayList();
						arrayListParameters.Add(new NpgsqlParameter("ketvalue", data.KlasifikasiArsip));
						arrayListParameters.Add(new NpgsqlParameter("penomoranId", data.Penomoranid));
						arrayListParameters.Add(new NpgsqlParameter("label", "klasArsip"));
						parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql, parameters);

						if (!string.IsNullOrEmpty(data.details))
						{
							foreach (var detail in data.details.Split('^'))
							{
								arrayListParameters = new ArrayList();
								var thisdetail = detail.Split('|');
								arrayListParameters.Add(new NpgsqlParameter("ketvalue", thisdetail[1]));
								arrayListParameters.Add(new NpgsqlParameter("penomoranId", data.Penomoranid));
								arrayListParameters.Add(new NpgsqlParameter("label", thisdetail[0]));
								parameters = arrayListParameters.OfType<object>().ToArray();
								ctx.Database.ExecuteSqlCommand(sql, parameters);
							}
						}
						tc.Commit();
						tr.Status = true;
						tr.Pesan = "Data Berhasil Disimpan";
					}
					catch (Exception ex)
					{
						tc.Rollback();
						tr.Pesan = ex.Message.ToString();
					}
					finally
					{
						tc.Dispose();
						ctx.Dispose();
					}
				}
			}
			return tr;
		}


		public List<KeteranganPenomoran> GetDetailsPenomoran(string penomoranid)
		{
			string skema = OtorisasiUser.NamaSkema;
			var datas = new List<KeteranganPenomoran>();
			var arrayListParameters = new ArrayList();

			using (var ctx = new PostgresDbContext())
			{
				string sql = $@"SELECT LABEL AS TEXTKETERANGAN, KETVALUE AS VALUEKETERANGAN
								FROM ""{skema}"".TBLKETPENOMORAN
								WHERE PENOMORANID = :penomoranid";
				arrayListParameters.Add(new NpgsqlParameter("penomoranid", penomoranid));

				var parameters = arrayListParameters.OfType<object>().ToArray();
				datas = ctx.Database.SqlQuery<KeteranganPenomoran>(sql, parameters).ToList();
			}
			return datas;
		}

		public decimal? GetNomorTerakhirPenomoran(string bkid, string jenisNaskah, string tahun)
		{
			string skema = OtorisasiUser.NamaSkema;
			decimal? nomor;
			var arrayListParameters = new ArrayList();
			var parameters = new object[0];
			using (var ctx = new PostgresDbContext())
			{
				string sql = $@"SELECT COUNTER
								FROM ""{skema}"".TBLCOUNTERPENOMORAN
								WHERE BUKUNOMORID = :bkid AND JENISNASKAHDINAS = :jenisNaskah AND TAHUNBERJALAN = :tahun";
				arrayListParameters.Add(new NpgsqlParameter("bkid", bkid));
				arrayListParameters.Add(new NpgsqlParameter("jenisNaskah", Uri.EscapeDataString(jenisNaskah)));
				arrayListParameters.Add(new NpgsqlParameter("tahun", int.Parse(tahun)));

				parameters = arrayListParameters.OfType<object>().ToArray();
				nomor = ctx.Database.SqlQuery<decimal?>(sql, parameters).FirstOrDefault();
			}

			return nomor;
		}

		public decimal? GetNomorOnDate(string bkid, string jenisNaskah, string tanggal, decimal nomorurut)
		{
			string skema = OtorisasiUser.NamaSkema;
			decimal? nomor;
			var arrayListParameters = new ArrayList();
			var parameters = new object[0];
			using (var ctx = new PostgresDbContext())
			{
				string sql = $@"SELECT BACKDATE
								FROM ""{skema}"".TBLPENOMORAN
								WHERE BUKUNOMORID = :bkid 
									AND (STATUSBATAL = '0' OR STATUSBATAL IS NULL) AND STATUS != 'BOOK'
									AND JENISNASKAHDINAS = :jenisNaskah
									AND TANGGALSURAT = TO_DATE(:tanggal,'DD/MM/YYYY')
									AND NOMORURUT = :nomorurut
								ORDER BY NOMORURUT DESC, BACKDATE DESC";
				arrayListParameters.Add(new NpgsqlParameter("bkid", bkid));
				arrayListParameters.Add(new NpgsqlParameter("jenisNaskah", Uri.EscapeDataString(jenisNaskah)));
				arrayListParameters.Add(new NpgsqlParameter("tanggal", tanggal));
				arrayListParameters.Add(new NpgsqlParameter("nomorurut", nomorurut));

				parameters = arrayListParameters.OfType<object>().ToArray();
				nomor = ctx.Database.SqlQuery<decimal?>(sql, parameters).FirstOrDefault();
			}

			return nomor;
		}

		public string PenomoranSuratBuilder(string nomorurut, string kodeTTD, string klasifikasiArsip, string jenisNaskahDinas, string TanggalSurat, bool ismenteri = false)
		{
			string format = string.Empty;
			kodeTTD = (string.IsNullOrEmpty(kodeTTD) ? "<KodeTTD>" : kodeTTD);
			var getTipe = getTipeSurat(jenisNaskahDinas);
			if (getTipe.Count > 0)
			{
				format = getTipe[0].FormatNomor;
				format = format.Replace("<Nomor>", nomorurut);
				format = format.Replace("<Arsip>", klasifikasiArsip);
				format = format.Replace("<Kode>", getTipe[0].KodeTipeSurat);

				if (jenisNaskahDinas == "Surat Dinas")
				{
					format = (ismenteri) ? format.Replace("-<Penandatangan>", "") : format.Replace("<Penandatangan>", kodeTTD);
				}
				else
				{
					format = (ismenteri) ? format.Replace("<Penandatangan>.", "") : format.Replace("<Penandatangan>", kodeTTD);
				}

				var date = DateTime.ParseExact(TanggalSurat, "dd/MM/yyyy", null);
				format = format.Replace("<Bulan>", new NaskahDinasModel().ToRoman(date.Month));
				format = format.Replace("<Tahun>", date.Year.ToString());
			}
			return format;
		}

		public List<UnitKerja> GetListUnitKerjaStruktural(bool pusat = false)
		{
			string skema = OtorisasiUser.NamaSkema;
			var arrayListParameters = new ArrayList();
			var list = new List<UnitKerja>();
			using (var ctx = new BpnDbContext())
			{
				string sql = $@"SELECT UNITKERJAID, INDUK, NAMAUNITKERJA, TIPEKANTORID, KODE
								FROM UNITKERJA
								WHERE
									LENGTH( UNITKERJAID ) = 6 
									AND TAMPIL = 1 
									AND INDUK IS NULL 
									{(pusat ? "AND KANTORID = '980FECFC746D8C80E0400B0A9214067D'" : "")}
								ORDER BY
									TIPEKANTORID,
									KODE,
									ESELON,
									NAMAUNITKERJA";

				list = ctx.Database.SqlQuery<UnitKerja>(sql).ToList();
			}

			return list;
		}

		public List<UnitKerja> GetListUnitKerjaStrukturalE2(string induk)
		{
			string skema = OtorisasiUser.NamaSkema;
			var arrayListParameters = new ArrayList();
			var list = new List<UnitKerja>();
			using (var ctx = new BpnDbContext())
			{
				string sql = $@"SELECT UNITKERJAID, INDUK, NAMAUNITKERJA, TIPEKANTORID, KODE
								FROM UNITKERJA
								WHERE TAMPIL = 1 
									AND (INDUK = :induk1 OR SUBSTR(UNITKERJAID, 0, 6) = :induk2 )
								ORDER BY
									TIPEKANTORID,
									KODE,
									ESELON,
									NAMAUNITKERJA";
				arrayListParameters.Add(new OracleParameter("induk1", induk));
				arrayListParameters.Add(new OracleParameter("induk2", induk));
				var parameters = arrayListParameters.OfType<object>().ToArray();
				list = ctx.Database.SqlQuery<UnitKerja>(sql, parameters).ToList();
			}

			return list;
		}

		public List<Jabatan> getListJabatan(string ukid = null, int? from = 0, int? to = 0, string ProfileId = null)
		{
			var datas = new List<Jabatan>();
			var arrayListParameters = new ArrayList();
			string sql = $@"SELECT
								ROW_NUMBER() OVER(ORDER BY JB.TIPEESELONID) RNUMBER,
								COUNT(1) OVER() AS TOTAL,
								JB.PROFILEID,
								JB.NAMA,
								JB.TIPEESELONID,
								CASE WHEN JB.KODETTD IS NULL THEN UK.KODE WHEN JB.KODETTD = '0' THEN UK.KODE WHEN JB.KODETTD != '0' THEN UK.KODE||'.'||JB.KODETTD ELSE UK.KODE END AS KODETTD,
								JB.UNITKERJAID,
								CASE WHEN JB.KODETTD IS NULL THEN '0' ELSE '1' END AS STATUS 
							FROM JABATAN JB
							LEFT JOIN UNITKERJA UK ON JB.UNITKERJAID = UK.UNITKERJAID 
							WHERE ( JB.NAMA LIKE 'Kepala%' OR JB.NAMA LIKE '%Jenderal%' OR JB.NAMA LIKE 'Direktur%' ) ";


			if (!string.IsNullOrEmpty(ProfileId))
			{
				sql += " AND JB.PROFILEID = :ProfileId ";
				arrayListParameters.Add(new OracleParameter("ProfileId", ProfileId));
			}
			else
			{
				sql += " AND(JB.UNITKERJAID = :ukid OR UK.INDUK = :ukid OR SUBSTR(JB.UNITKERJAID, 0, 6) = :ukid) ";
				arrayListParameters.Add(new OracleParameter("ukid", ukid));
				arrayListParameters.Add(new OracleParameter("ukid", ukid));
				arrayListParameters.Add(new OracleParameter("ukid", ukid));
			}

			sql += "ORDER BY JB.TIPEESELONID";

			if (to > 0)
			{
				sql = $"SELECT DATA.* FROM ({sql}) DATA WHERE DATA.RNUMBER BETWEEN :frmthis AND :tothis";
				arrayListParameters.Add(new OracleParameter("frmthis", from));
				arrayListParameters.Add(new OracleParameter("tothis", to));
			}

			using (var ctx = new BpnDbContext())
			{
				var parameters = arrayListParameters.OfType<object>().ToArray();
				datas = ctx.Database.SqlQuery<Jabatan>(sql, parameters).ToList();
			}

			return datas;
		}

		public findDokumenTTE findDokumenTTEbyNomor(string nomorsurat)
		{
			string skema = OtorisasiUser.NamaSkema;
			string dokTTE = string.Empty;
			var data = new findDokumenTTE();
			var arrayListParameters = new ArrayList();
			string sql = $@"SELECT T2.DOKUMENELEKTRONIKID, T2.STATUS
							FROM {skema}.TBLDOKUMENELEKTRONIK T1
							INNER JOIN {skema}.TBLDOKUMENTTE T2 
								ON T1.DOKUMENELEKTRONIKID = T2.DOKUMENELEKTRONIKID 
								AND T2.TIPE = 1
							WHERE UPPER(T1.NOMORSURAT) LIKE '%'||UPPER(:nomorsurat)||'%'
							ORDER BY T1.TANGGALDIBUAT DESC";
			arrayListParameters.Add(new OracleParameter("nomorsurat", nomorsurat));
			using (var ctx = new BpnDbContext())
			{
				var parameters = arrayListParameters.OfType<object>().ToArray();
				data = ctx.Database.SqlQuery<findDokumenTTE>(sql, parameters).FirstOrDefault();
			}

			return data;
		}

		public void simpanLogPengajuan(string penomoranid, string intext, string indetail)
		{
			string skema = OtorisasiUser.NamaSkema;
			var arrayListParameters = new ArrayList();
			string sql = $@"INSERT INTO ""{skema}"".LOGPENGAJUAN (PENOMORANID, INTEXT, INDETAIL, TANGGALUPDATE)
							VALUES (:penomoranid, :intext, :indetail, :tanggalupdate)";
			arrayListParameters.Add(new NpgsqlParameter("penomoranid", penomoranid));
			arrayListParameters.Add(new NpgsqlParameter("intext", intext));
			arrayListParameters.Add(new NpgsqlParameter("indetail", indetail));
			arrayListParameters.Add(new NpgsqlParameter("tanggalupdate", DateTime.Now));
			using (var ctx = new PostgresDbContext())
			{
				using (System.Data.Entity.DbContextTransaction tc = ctx.Database.BeginTransaction())
				{
					try
					{
						var parameters = arrayListParameters.OfType<object>().ToArray();
						ctx.Database.ExecuteSqlCommand(sql, parameters);
						tc.Commit();
					}
					catch (Exception ex)
					{
						tc.Rollback();
					}
				}
			}
		}

		public List<RiwayatPengajuan> getRiwayatPengajuan(string pegawaiid, string unitkerjaid, string bukuid, string StatArsip, int? from = 0, int? to = 0)
		{
			string skema = OtorisasiUser.NamaSkema;
			List<RiwayatPengajuan> datas = new List<RiwayatPengajuan>();
			var arrayListParameters = new ArrayList();
			string sql = $@"SELECT ROW_NUMBER() OVER(ORDER BY LOG.TANGGALUPDATE DESC) RNUMBER,
								   LOG.PENOMORANID, 
								   TO_CHAR(LOG.TANGGALUPDATE,'DD/MM/YYYY') AS TANGGALUPDATE, 
								   TP.NOMORSURAT, TP.JENISNASKAHDINAS, TP.PERIHAL,
								   TO_CHAR(TP.TANGGALSURAT,'DD/MM/YYYY') AS TANGGALSURAT, 
								   TP.STATUS, TP.STATUSBATAL, TP.KETERANGAN, TK.KETVALUE AS PROFILEPENANDATANGAN
							FROM ""{skema}"".LOGPENGAJUAN LOG
							INNER JOIN ""{skema}"".TBLPENOMORAN TP ON LOG.PENOMORANID = TP.PENOMORANID AND SPLIT_PART(LOG.INDETAIL,'|', 1) = :pegawaiid AND SPLIT_PART(LOG.INDETAIL,'|', 2) = :unitkerjaid AND TP.BUKUNOMORID = :bkid
							LEFT JOIN ""{skema}"".TBLKETPENOMORAN TK ON LOG.PENOMORANID = TK.PENOMORANID AND TK.LABEL = 'penandatangan'  ";


			if (!string.IsNullOrEmpty(StatArsip))
			{
				if (StatArsip == "TTE")
				{
					sql += " WHERE TP.KETERANGAN = :tte ";
					arrayListParameters.Add(new NpgsqlParameter("tte", StatArsip));
				}
				else if (StatArsip == "Batal")
				{
					sql += " WHERE TP.STATUSBATAL = :batal ";
					arrayListParameters.Add(new NpgsqlParameter("batal", "1"));
				}
				else
				{
					sql += " WHERE TP.STATUS = :stat AND TP.KETERANGAN = 'Manual'";
					arrayListParameters.Add(new NpgsqlParameter("stat", StatArsip));
				}

			}

			sql += " ORDER BY LOG.TANGGALUPDATE DESC";

			//if (to > 0)
			//{
			//	sql = $"SELECT DATA.* FROM ({sql}) AS DATA WHERE DATA.RNUMBER BETWEEN :frmthis AND :tothis";
			//	arrayListParameters.Add(new NpgsqlParameter("frmthis", from));
			//	arrayListParameters.Add(new NpgsqlParameter("tothis", to));
			//}

			arrayListParameters.Add(new NpgsqlParameter("pegawaiid", pegawaiid));
			arrayListParameters.Add(new NpgsqlParameter("unitkerjaid", unitkerjaid));
			arrayListParameters.Add(new NpgsqlParameter("bkid", bukuid));
			using (var ctx = new PostgresDbContext())
			{
				var parameters = arrayListParameters.OfType<object>().ToArray();
				datas = ctx.Database.SqlQuery<RiwayatPengajuan>(sql, parameters).ToList();
				foreach (var data in datas)
				{
					data.NomorSurat = (data.Status == "0" && data.Keterangan != "TTE") ? "" : Uri.UnescapeDataString(data.NomorSurat);
					data.JenisNaskahDinas = Uri.UnescapeDataString(data.JenisNaskahDinas);
					data.Perihal = Uri.UnescapeDataString(data.Perihal);
					data.NamaJabatan = new SuratModel().GetNamaJabatan(data.ProfilePenandatangan);
				}
			}
			return datas;
		}

		public List<DataPenomoran> getDataPenomoranExport(
		string bukunomorid,
		string jenis,
		string tahun,
		string searchKey = null,
		string Bulan = null,
		string StatArsip = null
		)
		{
			string skema = OtorisasiUser.NamaSkema;
			var datas = new List<DataPenomoran>();
			var arrayListParameters = new ArrayList();
			if (!string.IsNullOrEmpty(bukunomorid ?? jenis ?? tahun))
			{
				string sql = $@"SELECT ROW_NUMBER() OVER( ORDER BY TP.NOMORURUT DESC, TP.BACKDATE DESC ) RNUMBER,
									   COUNT(1) OVER() AS TOTAL,
									   TP.PENOMORANID,
									   TO_CHAR(TP.TANGGALINPUT,'DD/MM/YYYY HH24:MI') AS TANGGALINPUT,
									   TP.NOMORURUT,
									   TO_CHAR(TP.NOMORURUT, 'FM9999') || CASE WHEN TP.BACKDATE > 0 THEN '.' || TO_CHAR(TP.BACKDATE,'FM9999') ELSE '' END AS SNOMORURUT,
									   TO_CHAR(TP.TANGGALSURAT,'DD/MM/YYYY') AS TANGGALSURAT,
									   TP.NOMORSURAT,
									   TP.PERIHAL,
									   TP.JENISNASKAHDINAS,
									   TP.BUKUNOMORID,
									   TP.STATUS,
									   TP.KETERANGAN,
									   TP.STATUSBATAL,
									   TK.KETVALUE AS PROFILEPENANDATANGAN,
									   TK1.KETVALUE AS KLASIFIKASIARSIP,
									   TK2.KETVALUE AS PIC,
                                       TK3.KETVALUE AS UNITKERJA
									FROM
									   ""{skema}"".TBLPENOMORAN TP
									LEFT JOIN ""{skema}"".TBLKETPENOMORAN TK
									   ON TP.PENOMORANID = TK.PENOMORANID AND TK.LABEL = 'penandatangan'
									LEFT JOIN ""{skema}"".TBLKETPENOMORAN TK1
									   ON TP.PENOMORANID = TK1.PENOMORANID AND TK1.LABEL = 'klasArsip'
									LEFT JOIN ""{skema}"".TBLKETPENOMORAN TK2
									   ON TP.PENOMORANID = TK2.PENOMORANID AND TK2.LABEL = 'UserInput'
									LEFT JOIN ""{skema}"".TBLKETPENOMORAN TK3
									   ON TP.PENOMORANID = TK3.PENOMORANID AND TK3.LABEL = 'satkerInput'
									WHERE
									   TP.BUKUNOMORID = :bukunomorid AND TP.JENISNASKAHDINAS = :jenis AND EXTRACT(YEAR FROM TP.TANGGALSURAT) = :tahun 
									   AND STATUS != 'BOOK' ";
				arrayListParameters.Add(new NpgsqlParameter("bukunomorid", bukunomorid));
				arrayListParameters.Add(new NpgsqlParameter("jenis", jenis));
				arrayListParameters.Add(new NpgsqlParameter("tahun", int.Parse(tahun)));

				if (!string.IsNullOrEmpty(searchKey))
				{
					sql += " AND UPPER(NOMORSURAT) || UPPER(PERIHAL) || UPPER(TO_CHAR(TANGGALSURAT,'DD/MM/YYYY')) LIKE '%'||UPPER(:searchKey)||'%'  ";
					arrayListParameters.Add(new NpgsqlParameter("searchKey", searchKey));
				}

				if (!string.IsNullOrEmpty(Bulan))
				{
					sql += " AND TO_CHAR(EXTRACT(MONTH FROM TP.TANGGALSURAT),'FM9999') = :bulan ";
					arrayListParameters.Add(new NpgsqlParameter("bulan", Bulan));
				}

				if (!string.IsNullOrEmpty(StatArsip))
				{
					if (StatArsip == "TTE")
					{
						sql += " AND KETERANGAN = :tte ";
						arrayListParameters.Add(new NpgsqlParameter("tte", StatArsip));
					}
					else if (StatArsip == "Batal")
					{
						sql += " AND STATUSBATAL = :batal ";
						arrayListParameters.Add(new NpgsqlParameter("batal", "1"));
					}
					else
					{
						sql += " AND STATUS = :stat AND KETERANGAN = 'Manual'";
						arrayListParameters.Add(new NpgsqlParameter("stat", StatArsip));
					}

				}

				sql += " ORDER BY TP.NOMORURUT DESC, TP.BACKDATE DESC ";


				using (var ctx = new PostgresDbContext())
				{
					var parameters = arrayListParameters.OfType<object>().ToArray();
					datas = ctx.Database.SqlQuery<DataPenomoran>(sql, parameters).ToList();
				}

			}
			return datas;
		}


		public BukuPenomran getBukuFromPenandatangan(string profileid)
		{
			string skema = OtorisasiUser.NamaSkema;
			var buku = new BukuPenomran();
			var arrayListParameters = new ArrayList();

			using (var ctx = new PostgresDbContext())
			{
				var sql = $@"SELECT bukunomorid
							 FROM ""{skema}"".tblpenandatanganbuku 
							 WHERE profileid = :profileid AND statusaktif = '1'";
				arrayListParameters.Add(new NpgsqlParameter("profileid", profileid));
				var parameters = arrayListParameters.OfType<object>().ToArray();
				try
				{
					buku = ctx.Database.SqlQuery<BukuPenomran>(sql, parameters).FirstOrDefault();
				}
				catch
				{
					buku = new BukuPenomran();
				}
			}

			return buku;
		}

		public TransactionResult getInfoDokumenExtNomor(string pid, string type)
		{
			string skema = OtorisasiUser.NamaSkema;
			var result = new TransactionResult();
			var arrayListParameters = new ArrayList();
			var sql = "";
			using (var ctx = new PostgresDbContext())
			{
				if (type == "sisip")
				{
					sql = $@"SELECT ketvalue AS RETURNVALUE FROM ""{skema}"".tblketpenomoran WHERE penomoranid = :pid AND label = 'FileUpload' ";
				}
				else if(type == "batal")
                {
					sql = $@"SELECT ketvalue AS RETURNVALUE FROM ""{skema}"".tblketpenomoran WHERE penomoranid = :pid AND label = 'FileUploadBatal' ";
				}
				arrayListParameters.Add(new NpgsqlParameter("pid", pid));
				var parameters = arrayListParameters.OfType<object>().ToArray();
				try
				{
					result = ctx.Database.SqlQuery<TransactionResult>(sql, parameters).FirstOrDefault();
					if (!string.IsNullOrEmpty(result.ReturnValue))
					{
						result.Status = true;
						result.Pesan = "Sukses";
						result.ReturnValue2 = result.ReturnValue.Substring(0, result.ReturnValue.LastIndexOf('_'));
					}

				}
				catch
				{
					result = new TransactionResult() { Status = false, Pesan = "dokumen tidak ditemukan" };
				}
			}
			return result;
		}

		public string getKantoridforUpload(string pid)
		{
			string skema = OtorisasiUser.NamaSkema;
			var result = string.Empty;
			var arrayListParameters = new ArrayList();
			using (var ctx = new PostgresDbContext())
			{
				var sql = $@"SELECT ta.unitkerjaid
							 FROM ""{skema}"".tblpenomoran tp
							 INNER JOIN  ""{skema}"".tblketpenomoran tk 
								ON tk.penomoranid = tp.penomoranid 
								AND tk.label = 'UserInput' 
								AND tk.penomoranid = :pid
							 INNER JOIN  ""{skema}"".tblaksesbuku ta
								ON ta.bukunomorid = tp.bukunomorid
								AND ta.pegawaiid = tk.ketvalue  ";
				arrayListParameters.Add(new NpgsqlParameter("pid", pid));
				var parameters = arrayListParameters.OfType<object>().ToArray();
				result = ctx.Database.SqlQuery<string>(sql, parameters).FirstOrDefault();
			}
			return result;
		}
	}
}