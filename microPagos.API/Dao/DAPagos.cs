using microPagos.API.Model.Response;
using microPagos.API.Utils;
using MySql.Data.MySqlClient;

namespace microPagos.API.Dao
{
    public class DAPagos
    {
        public static bool RegistrarIntentoPago(decimal monto, int idPedido, int idPasarela, int idUsuario)
        {
            using (MySqlConnection conn = new MySqlConnection(Variables.Conexion.cnx))
            {
                try
                {
                    conn.Open();

                    string sql = @"
                                INSERT INTO tbl_cer_pago 
                                (
                                    cer_decimal_monto,
                                    cer_enum_estado,
                                    cer_int_id_pedido,
                                    cer_int_id_pasarela,
                                    cer_int_created_by,
                                    cer_datetime_created_at
                                )
                                VALUES
                                (
                                    @Monto,
                                    @Estado,
                                    @IdPedido,
                                    @IdPasarela,
                                    @IdUsuario,
                                    NOW()
                                );";

                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Monto", monto);
                    cmd.Parameters.AddWithValue("@Estado", "Pendiente");
                    cmd.Parameters.AddWithValue("@IdPedido", idPedido);
                    cmd.Parameters.AddWithValue("@IdPasarela", idPasarela);
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                    cmd.ExecuteNonQuery();

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al registrar intento de pago: {ex.Message}");
                    return false;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static int CrearPasarela(string nombre, int idUsuarioCreador)
        {
            using (MySqlConnection conn = new MySqlConnection(Variables.Conexion.cnx))
            {
                try
                {
                    conn.Open();

                    string sql = @"
                                    INSERT INTO tbl_cer_pasarela
                                    (
                                        cer_varchar_nombre,
                                        cer_int_created_by
                                    )
                                    VALUES
                                    (
                                        @Nombre,
                                        @IdUsuario
                                    );
                                    SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", nombre);
                        cmd.Parameters.AddWithValue("@IdUsuario", idUsuarioCreador);

                        object result = cmd.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : 0;
                    }
                }
                catch (Exception ex)
                {
                    return 0; 
                }
            }
        }
        public static List<Municipio> ObtenerMunicipios()
        {
            List<Municipio> municipios = new List<Municipio>();

            using (MySqlConnection conn = new MySqlConnection(Variables.Conexion.cnx))
            {
                try
                {
                    conn.Open();

                    string sql = @"
                                    SELECT 
                                        m.cer_int_id_municipio AS IdMunicipio,
                                        d.cer_int_id_departamento AS IdDepartamento,
                                        CONCAT(m.cer_vch_nombre, ' / ', d.cer_vch_nombre) AS Nombre
                                    FROM tbl_cer_municipios m
                                    INNER JOIN tbl_cer_departamentos d 
                                        ON m.cer_int_id_departamento = d.cer_int_id_departamento
                                    ORDER BY d.cer_vch_nombre, m.cer_vch_nombre;
                                ";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            municipios.Add(new Municipio
                            {
                                IdMunicipio = reader.GetInt32("IdMunicipio"),
                                IdDepartamento = reader.GetInt32("IdDepartamento"),
                                Nombre = reader.GetString("Nombre")
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al obtener municipios: {ex.Message}");
                }
                finally
                {
                    conn.Close();
                }
            }

            return municipios;
        }
        public static Municipio ObtenerMunicipioPorId(int idMunicipio)
        {
            using (MySqlConnection conn = new MySqlConnection(Variables.Conexion.cnx))
            {
                try
                {
                    conn.Open();

                    string sql = @"
                                    SELECT 
                                        cer_int_id_municipio AS IdMunicipio,
                                        cer_vch_nombre AS Nombre,
                                        cer_bit_es_capital AS EsCapital,
                                        cer_vch_lat AS Lat,
                                        cer_vch_lon AS Lon
                                    FROM tbl_cer_municipios
                                    WHERE cer_int_id_municipio = @idMunicipio
                                    LIMIT 1;
                                ";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@idMunicipio", idMunicipio);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Municipio
                                {
                                    IdMunicipio = reader.GetInt32("IdMunicipio"),
                                    Nombre = reader.GetString("Nombre"),
                                    EsCapital = reader.GetBoolean("EsCapital"),
                                    Latitud = reader.IsDBNull(reader.GetOrdinal("Lat")) ? 0 : Convert.ToDouble(reader["Lat"]),
                                    Longitud = reader.IsDBNull(reader.GetOrdinal("Lon")) ? 0 : Convert.ToDouble(reader["Lon"])
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return null;
        }
    }
}
