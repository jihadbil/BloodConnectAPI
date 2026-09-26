using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloodConnectAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixBloodRequestForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloodDisbursements_BloodRequests_BloodRequestRequestID",
                table: "BloodDisbursements");

            migrationBuilder.DropForeignKey(
                name: "FK_DonorRequestResponses_BloodRequests_BloodRequestRequestID",
                table: "DonorRequestResponses");

            migrationBuilder.DropIndex(
                name: "IX_DonorRequestResponses_BloodRequestRequestID",
                table: "DonorRequestResponses");

            migrationBuilder.DropIndex(
                name: "IX_BloodDisbursements_BloodRequestRequestID",
                table: "BloodDisbursements");

            migrationBuilder.DropColumn(
                name: "BloodRequestRequestID",
                table: "DonorRequestResponses");

            migrationBuilder.DropColumn(
                name: "BloodRequestRequestID",
                table: "BloodDisbursements");

            migrationBuilder.CreateIndex(
                name: "IX_DonorRequestResponses_RequestID",
                table: "DonorRequestResponses",
                column: "RequestID");

            migrationBuilder.CreateIndex(
                name: "IX_BloodDisbursements_RequestID",
                table: "BloodDisbursements",
                column: "RequestID");

            migrationBuilder.AddForeignKey(
                name: "FK_BloodDisbursements_BloodRequests_RequestID",
                table: "BloodDisbursements",
                column: "RequestID",
                principalTable: "BloodRequests",
                principalColumn: "RequestID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DonorRequestResponses_BloodRequests_RequestID",
                table: "DonorRequestResponses",
                column: "RequestID",
                principalTable: "BloodRequests",
                principalColumn: "RequestID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloodDisbursements_BloodRequests_RequestID",
                table: "BloodDisbursements");

            migrationBuilder.DropForeignKey(
                name: "FK_DonorRequestResponses_BloodRequests_RequestID",
                table: "DonorRequestResponses");

            migrationBuilder.DropIndex(
                name: "IX_DonorRequestResponses_RequestID",
                table: "DonorRequestResponses");

            migrationBuilder.DropIndex(
                name: "IX_BloodDisbursements_RequestID",
                table: "BloodDisbursements");

            migrationBuilder.AddColumn<int>(
                name: "BloodRequestRequestID",
                table: "DonorRequestResponses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BloodRequestRequestID",
                table: "BloodDisbursements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DonorRequestResponses_BloodRequestRequestID",
                table: "DonorRequestResponses",
                column: "BloodRequestRequestID");

            migrationBuilder.CreateIndex(
                name: "IX_BloodDisbursements_BloodRequestRequestID",
                table: "BloodDisbursements",
                column: "BloodRequestRequestID");

            migrationBuilder.AddForeignKey(
                name: "FK_BloodDisbursements_BloodRequests_BloodRequestRequestID",
                table: "BloodDisbursements",
                column: "BloodRequestRequestID",
                principalTable: "BloodRequests",
                principalColumn: "RequestID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DonorRequestResponses_BloodRequests_BloodRequestRequestID",
                table: "DonorRequestResponses",
                column: "BloodRequestRequestID",
                principalTable: "BloodRequests",
                principalColumn: "RequestID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
