using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pedidos360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFotoBinariaProducto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "Productos");

            migrationBuilder.AddColumn<string>(
                name: "ImagenContentType",
                table: "Productos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ImagenData",
                table: "Productos",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagenContentType",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "ImagenData",
                table: "Productos");

            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "Productos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
