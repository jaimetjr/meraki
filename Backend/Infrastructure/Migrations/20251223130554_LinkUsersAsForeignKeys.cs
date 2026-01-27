using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkUsersAsForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Therapists_CreatedBy",
                table: "Therapists",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Therapists_UpdatedBy",
                table: "Therapists",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_CreatedBy",
                table: "Testimonials",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_UpdatedBy",
                table: "Testimonials",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Specialties_CreatedBy",
                table: "Specialties",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Specialties_UpdatedBy",
                table: "Specialties",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Services_CreatedBy",
                table: "Services",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Services_UpdatedBy",
                table: "Services",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CreatedBy",
                table: "Courses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_UpdatedBy",
                table: "Courses",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CreatedBy",
                table: "Categories",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UpdatedBy",
                table: "Categories",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Benefits_CreatedBy",
                table: "Benefits",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Benefits_UpdatedBy",
                table: "Benefits",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Users_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Benefits_Users_CreatedBy",
                table: "Benefits",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Benefits_Users_UpdatedBy",
                table: "Benefits",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_CreatedBy",
                table: "Categories",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_UpdatedBy",
                table: "Categories",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_CreatedBy",
                table: "Courses",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_UpdatedBy",
                table: "Courses",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Users_CreatedBy",
                table: "Services",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Users_UpdatedBy",
                table: "Services",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Specialties_Users_CreatedBy",
                table: "Specialties",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Specialties_Users_UpdatedBy",
                table: "Specialties",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Testimonials_Users_CreatedBy",
                table: "Testimonials",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Testimonials_Users_UpdatedBy",
                table: "Testimonials",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Therapists_Users_CreatedBy",
                table: "Therapists",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Therapists_Users_UpdatedBy",
                table: "Therapists",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Users_UserId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Benefits_Users_CreatedBy",
                table: "Benefits");

            migrationBuilder.DropForeignKey(
                name: "FK_Benefits_Users_UpdatedBy",
                table: "Benefits");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_CreatedBy",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_UpdatedBy",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_CreatedBy",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_UpdatedBy",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Users_CreatedBy",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Users_UpdatedBy",
                table: "Services");

            migrationBuilder.DropForeignKey(
                name: "FK_Specialties_Users_CreatedBy",
                table: "Specialties");

            migrationBuilder.DropForeignKey(
                name: "FK_Specialties_Users_UpdatedBy",
                table: "Specialties");

            migrationBuilder.DropForeignKey(
                name: "FK_Testimonials_Users_CreatedBy",
                table: "Testimonials");

            migrationBuilder.DropForeignKey(
                name: "FK_Testimonials_Users_UpdatedBy",
                table: "Testimonials");

            migrationBuilder.DropForeignKey(
                name: "FK_Therapists_Users_CreatedBy",
                table: "Therapists");

            migrationBuilder.DropForeignKey(
                name: "FK_Therapists_Users_UpdatedBy",
                table: "Therapists");

            migrationBuilder.DropIndex(
                name: "IX_Therapists_CreatedBy",
                table: "Therapists");

            migrationBuilder.DropIndex(
                name: "IX_Therapists_UpdatedBy",
                table: "Therapists");

            migrationBuilder.DropIndex(
                name: "IX_Testimonials_CreatedBy",
                table: "Testimonials");

            migrationBuilder.DropIndex(
                name: "IX_Testimonials_UpdatedBy",
                table: "Testimonials");

            migrationBuilder.DropIndex(
                name: "IX_Specialties_CreatedBy",
                table: "Specialties");

            migrationBuilder.DropIndex(
                name: "IX_Specialties_UpdatedBy",
                table: "Specialties");

            migrationBuilder.DropIndex(
                name: "IX_Services_CreatedBy",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_UpdatedBy",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Courses_CreatedBy",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_UpdatedBy",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CreatedBy",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_UpdatedBy",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Benefits_CreatedBy",
                table: "Benefits");

            migrationBuilder.DropIndex(
                name: "IX_Benefits_UpdatedBy",
                table: "Benefits");
        }
    }
}
