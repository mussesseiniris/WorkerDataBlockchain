using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace wdb_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "certification_file_name",
                table: "employer",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "certification_file_path",
                table: "employer",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "certification_status",
                table: "employer",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "certification_uploaded_at",
                table: "employer",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_request_employer_id",
                table: "request",
                column: "employer_id");

            migrationBuilder.CreateIndex(
                name: "IX_request_worker_id",
                table: "request",
                column: "worker_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_recipient_employer_id",
                table: "notification",
                column: "recipient_employer_id");

            migrationBuilder.CreateIndex(
                name: "IX_notification_recipient_worker_id",
                table: "notification",
                column: "recipient_worker_id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_notification_type",
                table: "notification",
                sql: "type IN ('NEW_REQUEST', 'DATA_ACCESSED', 'REQUEST_REVIEWED', 'ACCESS_REVOKED')");

            migrationBuilder.AddForeignKey(
                name: "FK_notification_employer_recipient_employer_id",
                table: "notification",
                column: "recipient_employer_id",
                principalTable: "employer",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_notification_worker_recipient_worker_id",
                table: "notification",
                column: "recipient_worker_id",
                principalTable: "worker",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_request_employer_employer_id",
                table: "request",
                column: "employer_id",
                principalTable: "employer",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_request_worker_worker_id",
                table: "request",
                column: "worker_id",
                principalTable: "worker",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notification_employer_recipient_employer_id",
                table: "notification");

            migrationBuilder.DropForeignKey(
                name: "FK_notification_worker_recipient_worker_id",
                table: "notification");

            migrationBuilder.DropForeignKey(
                name: "FK_request_employer_employer_id",
                table: "request");

            migrationBuilder.DropForeignKey(
                name: "FK_request_worker_worker_id",
                table: "request");

            migrationBuilder.DropIndex(
                name: "IX_request_employer_id",
                table: "request");

            migrationBuilder.DropIndex(
                name: "IX_request_worker_id",
                table: "request");

            migrationBuilder.DropIndex(
                name: "IX_notification_recipient_employer_id",
                table: "notification");

            migrationBuilder.DropIndex(
                name: "IX_notification_recipient_worker_id",
                table: "notification");

            migrationBuilder.DropCheckConstraint(
                name: "CK_notification_type",
                table: "notification");

            migrationBuilder.DropColumn(
                name: "certification_file_name",
                table: "employer");

            migrationBuilder.DropColumn(
                name: "certification_file_path",
                table: "employer");

            migrationBuilder.DropColumn(
                name: "certification_status",
                table: "employer");

            migrationBuilder.DropColumn(
                name: "certification_uploaded_at",
                table: "employer");
        }
    }
}
