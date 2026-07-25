using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgresSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "contacts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    is_handled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contacts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "job_positions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "citext", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_positions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "citext", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "citext", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    security_stamp = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    user_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "employers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    company_phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    web_site = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    number_of_employees = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    sectors = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employers", x => x.id);
                    table.ForeignKey(
                        name: "fk_employers_users_id",
                        column: x => x.id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "job_seekers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    national_id = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_seekers", x => x.id);
                    table.ForeignKey(
                        name: "fk_job_seekers_users_id",
                        column: x => x.id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_by_ip = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    replaced_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "system_staff",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_staff", x => x.id);
                    table.ForeignKey(
                        name: "fk_system_staff_users_id",
                        column: x => x.id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employer_departments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    number_of_employees = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employer_departments", x => x.id);
                    table.ForeignKey(
                        name: "fk_employer_departments_employers_employer_id",
                        column: x => x.employer_id,
                        principalTable: "employers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "job_advertisements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_position_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    experience = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    skills = table.Column<string[]>(type: "text[]", nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    min_salary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    max_salary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    open_positions = table.Column<int>(type: "integer", nullable: false),
                    job_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    deadline = table.Column<DateOnly>(type: "date", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_advertisements", x => x.id);
                    table.CheckConstraint("ck_job_advertisements_open_positions", "open_positions > 0");
                    table.CheckConstraint("ck_job_advertisements_salary_range", "max_salary IS NULL OR min_salary IS NULL OR max_salary >= min_salary");
                    table.ForeignKey(
                        name: "fk_job_advertisements_employers_employer_id",
                        column: x => x.employer_id,
                        principalTable: "employers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_job_advertisements_job_positions_job_position_id",
                        column: x => x.job_position_id,
                        principalTable: "job_positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cvs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_seeker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    information = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    hobbies = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    skills = table.Column<string[]>(type: "text[]", nullable: false),
                    social_github = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    social_linkedin = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    social_website = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cvs", x => x.id);
                    table.ForeignKey(
                        name: "fk_cvs_job_seekers_job_seeker_id",
                        column: x => x.job_seeker_id,
                        principalTable: "job_seekers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "job_applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_advertisement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_seeker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_seeker_note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    employer_note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    status_changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_applications", x => x.id);
                    table.ForeignKey(
                        name: "fk_job_applications_job_advertisements_job_advertisement_id",
                        column: x => x.job_advertisement_id,
                        principalTable: "job_advertisements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_job_applications_job_seekers_job_seeker_id",
                        column: x => x.job_seeker_id,
                        principalTable: "job_seekers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cv_educations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cv_id = table.Column<Guid>(type: "uuid", nullable: false),
                    school = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    major = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    grade = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    start_year = table.Column<int>(type: "integer", nullable: true),
                    end_year = table.Column<int>(type: "integer", nullable: true),
                    is_graduated = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cv_educations", x => x.id);
                    table.ForeignKey(
                        name: "fk_cv_educations_cvs_cv_id",
                        column: x => x.cv_id,
                        principalTable: "cvs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cv_files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cv_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    storage_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    storage_provider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    content_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cv_files", x => x.id);
                    table.ForeignKey(
                        name: "fk_cv_files_cvs_cv_id",
                        column: x => x.cv_id,
                        principalTable: "cvs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cv_job_experiences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cv_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    department = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    position = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    start_year = table.Column<int>(type: "integer", nullable: true),
                    end_year = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cv_job_experiences", x => x.id);
                    table.ForeignKey(
                        name: "fk_cv_job_experiences_cvs_cv_id",
                        column: x => x.cv_id,
                        principalTable: "cvs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cv_languages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cv_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    level = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cv_languages", x => x.id);
                    table.ForeignKey(
                        name: "fk_cv_languages_cvs_cv_id",
                        column: x => x.cv_id,
                        principalTable: "cvs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cv_projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cv_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cv_projects", x => x.id);
                    table.ForeignKey(
                        name: "fk_cv_projects_cvs_cv_id",
                        column: x => x.cv_id,
                        principalTable: "cvs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_contacts_is_handled",
                table: "contacts",
                column: "is_handled");

            migrationBuilder.CreateIndex(
                name: "ix_cv_educations_cv_id",
                table: "cv_educations",
                column: "cv_id");

            migrationBuilder.CreateIndex(
                name: "ix_cv_files_cv_id",
                table: "cv_files",
                column: "cv_id");

            migrationBuilder.CreateIndex(
                name: "ix_cv_job_experiences_cv_id",
                table: "cv_job_experiences",
                column: "cv_id");

            migrationBuilder.CreateIndex(
                name: "ix_cv_languages_cv_id",
                table: "cv_languages",
                column: "cv_id");

            migrationBuilder.CreateIndex(
                name: "ix_cv_projects_cv_id",
                table: "cv_projects",
                column: "cv_id");

            migrationBuilder.CreateIndex(
                name: "ix_cvs_job_seeker_id",
                table: "cvs",
                column: "job_seeker_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cvs_skills",
                table: "cvs",
                column: "skills")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_employer_departments_employer_id",
                table: "employer_departments",
                column: "employer_id");

            migrationBuilder.CreateIndex(
                name: "ix_job_advertisements_city",
                table: "job_advertisements",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "ix_job_advertisements_employer_id",
                table: "job_advertisements",
                column: "employer_id");

            migrationBuilder.CreateIndex(
                name: "ix_job_advertisements_is_active_deadline",
                table: "job_advertisements",
                columns: new[] { "is_active", "deadline" });

            migrationBuilder.CreateIndex(
                name: "ix_job_advertisements_job_position_id",
                table: "job_advertisements",
                column: "job_position_id");

            migrationBuilder.CreateIndex(
                name: "ix_job_advertisements_skills",
                table: "job_advertisements",
                column: "skills")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_job_applications_job_advertisement_id",
                table: "job_applications",
                column: "job_advertisement_id");

            migrationBuilder.CreateIndex(
                name: "ix_job_applications_job_seeker_id_job_advertisement_id",
                table: "job_applications",
                columns: new[] { "job_seeker_id", "job_advertisement_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_job_positions_name",
                table: "job_positions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_job_seekers_national_id",
                table: "job_seekers",
                column: "national_id",
                unique: true,
                filter: "national_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id_expires_at",
                table: "refresh_tokens",
                columns: new[] { "user_id", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contacts");

            migrationBuilder.DropTable(
                name: "cv_educations");

            migrationBuilder.DropTable(
                name: "cv_files");

            migrationBuilder.DropTable(
                name: "cv_job_experiences");

            migrationBuilder.DropTable(
                name: "cv_languages");

            migrationBuilder.DropTable(
                name: "cv_projects");

            migrationBuilder.DropTable(
                name: "employer_departments");

            migrationBuilder.DropTable(
                name: "job_applications");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "system_staff");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "cvs");

            migrationBuilder.DropTable(
                name: "job_advertisements");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "job_seekers");

            migrationBuilder.DropTable(
                name: "employers");

            migrationBuilder.DropTable(
                name: "job_positions");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
