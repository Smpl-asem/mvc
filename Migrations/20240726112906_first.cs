using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace test.Migrations
{
    /// <inheritdoc />
    public partial class first : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permission_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission_tbl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role_tbl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sms_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SmsCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TryCount = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sms_tbl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "smsTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_smsTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Addres = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NatinalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PerconalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Profile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users_tbl", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions_tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_tbl_Permission_tbl_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permission_tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_tbl_Role_tbl_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role_tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderUserId = table.Column<int>(type: "int", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BodyText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Trashed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages_tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_tbl_Users_tbl_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users_tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "userLogs_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LogAction = table.Column<int>(type: "int", nullable: false),
                    isSucces = table.Column<bool>(type: "bit", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userLogs_tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_userLogs_tbl_Users_tbl_UserId",
                        column: x => x.UserId,
                        principalTable: "Users_tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles_tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_tbl_Role_tbl_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role_tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_tbl_Users_tbl_UserId",
                        column: x => x.UserId,
                        principalTable: "Users_tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attecheds_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MessageId = table.Column<int>(type: "int", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attecheds_tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attecheds_tbl_Messages_tbl_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages_tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "msgLog_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LogAction = table.Column<int>(type: "int", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_msgLog_tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_msgLog_tbl_Messages_tbl_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages_tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_msgLog_tbl_Users_tbl_UserId",
                        column: x => x.UserId,
                        principalTable: "Users_tbl",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recivers_tbl",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReciverId = table.Column<int>(type: "int", nullable: true),
                    MessageId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recivers_tbl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recivers_tbl_Messages_tbl_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages_tbl",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Recivers_tbl_Users_tbl_ReciverId",
                        column: x => x.ReciverId,
                        principalTable: "Users_tbl",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attecheds_tbl_MessageId",
                table: "Attecheds_tbl",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_tbl_SenderUserId",
                table: "Messages_tbl",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_msgLog_tbl_MessageId",
                table: "msgLog_tbl",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_msgLog_tbl_UserId",
                table: "msgLog_tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Recivers_tbl_MessageId",
                table: "Recivers_tbl",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Recivers_tbl_ReciverId",
                table: "Recivers_tbl",
                column: "ReciverId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_tbl_PermissionId",
                table: "RolePermissions_tbl",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_tbl_RoleId",
                table: "RolePermissions_tbl",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_userLogs_tbl_UserId",
                table: "userLogs_tbl",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_tbl_RoleId",
                table: "UserRoles_tbl",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_tbl_UserId",
                table: "UserRoles_tbl",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attecheds_tbl");

            migrationBuilder.DropTable(
                name: "msgLog_tbl");

            migrationBuilder.DropTable(
                name: "Recivers_tbl");

            migrationBuilder.DropTable(
                name: "RolePermissions_tbl");

            migrationBuilder.DropTable(
                name: "sms_tbl");

            migrationBuilder.DropTable(
                name: "smsTokens");

            migrationBuilder.DropTable(
                name: "userLogs_tbl");

            migrationBuilder.DropTable(
                name: "UserRoles_tbl");

            migrationBuilder.DropTable(
                name: "Messages_tbl");

            migrationBuilder.DropTable(
                name: "Permission_tbl");

            migrationBuilder.DropTable(
                name: "Role_tbl");

            migrationBuilder.DropTable(
                name: "Users_tbl");
        }
    }
}
