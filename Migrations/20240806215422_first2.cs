using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace test.Migrations
{
    /// <inheritdoc />
    public partial class first2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttechedReplies_tbl_Messages_tbl_ReplyId",
                table: "AttechedReplies_tbl");

            migrationBuilder.DropForeignKey(
                name: "FK_AttechedReplies_tbl_Reply_tbl_ReplyId1",
                table: "AttechedReplies_tbl");

            migrationBuilder.DropIndex(
                name: "IX_AttechedReplies_tbl_ReplyId1",
                table: "AttechedReplies_tbl");

            migrationBuilder.DropColumn(
                name: "ReplyId1",
                table: "AttechedReplies_tbl");

            migrationBuilder.AddForeignKey(
                name: "FK_AttechedReplies_tbl_Reply_tbl_ReplyId",
                table: "AttechedReplies_tbl",
                column: "ReplyId",
                principalTable: "Reply_tbl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttechedReplies_tbl_Reply_tbl_ReplyId",
                table: "AttechedReplies_tbl");

            migrationBuilder.AddColumn<int>(
                name: "ReplyId1",
                table: "AttechedReplies_tbl",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttechedReplies_tbl_ReplyId1",
                table: "AttechedReplies_tbl",
                column: "ReplyId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AttechedReplies_tbl_Messages_tbl_ReplyId",
                table: "AttechedReplies_tbl",
                column: "ReplyId",
                principalTable: "Messages_tbl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AttechedReplies_tbl_Reply_tbl_ReplyId1",
                table: "AttechedReplies_tbl",
                column: "ReplyId1",
                principalTable: "Reply_tbl",
                principalColumn: "Id");
        }
    }
}
