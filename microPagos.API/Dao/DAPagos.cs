using microPagos.API.Utils;
using MySql.Data.MySqlClient;

namespace microPagos.API.Dao
{
    public class DAPagos
    {
        public static bool GuardarPago(decimal monto, int idPedido, int idPasarela, int idUsuario)
        
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
                            cer_int_created_by
                        )
                        VALUES
                        (
                            @Monto,
                            @Estado,
                            @IdPedido,
                            @IdPasarela,
                            @IdUsuario
                        );";

                    var cmd = new MySqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Monto", monto);
                    cmd.Parameters.AddWithValue("@Estado", "Pendiente"); // inicia en Pendiente
                    cmd.Parameters.AddWithValue("@IdPedido", idPedido);
                    cmd.Parameters.AddWithValue("@IdPasarela", idPasarela);
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);

                    cmd.ExecuteNonQuery();

                    return true;
                }
                catch (Exception ex)
                {
                    // Log del error (puedes usar tu clase Logger o Console)
                    Console.WriteLine($"Error al guardar pago: {ex.Message}");
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

    }
}
