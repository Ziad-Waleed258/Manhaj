using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manhaj.Migrations
{
    /// <inheritdoc />
    public partial class AddingQuizzes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Student_Quiz_Answers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    QuizId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    SelectedOptionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student_Quiz_Answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Student_Quiz_Answers_Options_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "Options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Student_Quiz_Answers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Student_Quiz_Answers_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Student_Quiz_Answers_Student_Quizzes_StudentId_QuizId",
                        columns: x => new { x.StudentId, x.QuizId },
                        principalTable: "Student_Quizzes",
                        principalColumns: new[] { "StudentId", "QuizId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Student_Quiz_Answers_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Student_Quiz_Answers_QuestionId",
                table: "Student_Quiz_Answers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Quiz_Answers_QuizId",
                table: "Student_Quiz_Answers",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Quiz_Answers_SelectedOptionId",
                table: "Student_Quiz_Answers",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Student_Quiz_Answers_StudentId_QuizId",
                table: "Student_Quiz_Answers",
                columns: new[] { "StudentId", "QuizId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Student_Quiz_Answers");
        }
    }
}
