using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Egost.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromoCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Percent = table.Column<int>(type: "int", nullable: false),
                    MaxSaleCents = table.Column<decimal>(type: "decimal(20,0)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    SKU = table.Column<long>(type: "bigint", nullable: false),
                    Views = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    PriceCents = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    SalePercent = table.Column<int>(type: "int", nullable: false),
                    Warranty = table.Column<TimeSpan>(type: "time", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromoCodeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carts_PromoCodes_PromoCodeId",
                        column: x => x.PromoCodeId,
                        principalTable: "PromoCodes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DOB = table.Column<DateOnly>(type: "date", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    CartId = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    CartId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartProducts_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CartProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoreAddress = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductUser",
                columns: table => new
                {
                    WishListId = table.Column<int>(type: "int", nullable: false),
                    WishlistUsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductUser", x => new { x.WishListId, x.WishlistUsersId });
                    table.ForeignKey(
                        name: "FK_ProductUser_AspNetUsers_WishlistUsersId",
                        column: x => x.WishlistUsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductUser_Products_WishListId",
                        column: x => x.WishListId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<byte>(type: "tinyint", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_AspNetUsers_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Searches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    KeyWord = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Searches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Searches_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Searches_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TransporterId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PromoCodeId = table.Column<int>(type: "int", nullable: true),
                    TotalCents = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryNeeded = table.Column<bool>(type: "bit", nullable: false),
                    Processed = table.Column<bool>(type: "bit", nullable: false),
                    PaymobOrderId = table.Column<int>(type: "int", nullable: true),
                    AddressId = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_TransporterId",
                        column: x => x.TransporterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_PromoCodes_PromoCodeId",
                        column: x => x.PromoCodeId,
                        principalTable: "PromoCodes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EditHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EditorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Field = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EditDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AddressId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    PromoCodeId = table.Column<int>(type: "int", nullable: true),
                    ReviewId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EditHistories_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EditHistories_AspNetUsers_EditorId",
                        column: x => x.EditorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EditHistories_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EditHistories_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EditHistories_PromoCodes_PromoCodeId",
                        column: x => x.PromoCodeId,
                        principalTable: "PromoCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EditHistories_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrderProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductPriceCents = table.Column<decimal>(type: "decimal(20,0)", nullable: false),
                    SalePercent = table.Column<float>(type: "real", nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    Warranty = table.Column<TimeSpan>(type: "time", nullable: false),
                    PartiallyOrFullyReturnedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderProducts_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReturnProductOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransporterId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    OrderProductId = table.Column<int>(type: "int", nullable: false),
                    ReturnReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnProductOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnProductOrders_AspNetUsers_TransporterId",
                        column: x => x.TransporterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReturnProductOrders_OrderProducts_OrderProductId",
                        column: x => x.OrderProductId,
                        principalTable: "OrderProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReturnProductOrders_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Addresses",
                columns: new[] { "Id", "AddressLine1", "AddressLine2", "City", "Country", "CreatedDateTime", "DeletedDateTime", "PostalCode", "StoreAddress", "Telephone", "UserId" },
                values: new object[] { 1, "Base", null, "Base", "Base", new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(500), null, "Base", true, "Base", null });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Sports, Instruments & Accessories" },
                    { 2, "Toys, Games, Video Games & Accessories" },
                    { 3, "Arts, Crafts & Sewing" },
                    { 4, "Clothing, Shoes & Jewelry" },
                    { 5, "Beauty & Personal Care" },
                    { 6, "Books" },
                    { 7, "Electronics & Accessories" },
                    { 8, "Software" },
                    { 9, "Grocery & Gourmet Food" },
                    { 10, "Home Furniture & Accessories" },
                    { 11, "Luggage & Travel Gear" },
                    { 12, "Pet Supplies" }
                });

            migrationBuilder.InsertData(
                table: "PromoCodes",
                columns: new[] { "Id", "Active", "Code", "CreatedDateTime", "DeletedDateTime", "Description", "MaxSaleCents", "Percent" },
                values: new object[,]
                {
                    { 1, true, "SUMMER2024", new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(832), null, "SUMMER2024", 5000m, 10 },
                    { 2, true, "WELCOME10", new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(835), null, "WELCOME10", null, 10 },
                    { 3, true, "HOLIDAY25", new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(838), null, "HOLIDAY25", 15000m, 25 },
                    { 4, true, "SPRING2024", new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(841), null, "SPRING2024", 8000m, 15 }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedDateTime", "DeletedDateTime", "Description", "Name", "PriceCents", "SKU", "SalePercent", "Views", "Warranty" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(706), null, "High-quality tennis racket for professionals.", "Wilson Tennis Racket", 8999m, 10001L, 10, 0m, new TimeSpan(730, 0, 0, 0, 0) },
                    { 2, 1, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(710), null, "Top-notch acoustic guitar with a smooth finish.", "Yamaha Acoustic Guitar", 14999m, 10002L, 15, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 3, 1, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(714), null, "Latest EA sports soccer game ps5 edition.", "EA sports FC24 for PS5", 12999m, 10003L, 5, 0m, new TimeSpan(14, 0, 0, 0, 0) },
                    { 4, 1, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(717), null, "Official size soccer ball for all levels.", "Adidas Soccer Ball", 2999m, 10004L, 0, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 5, 1, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(720), null, "Complete badminton set for backyard fun.", "Wilson Badminton Set", 4599m, 10005L, 0, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 6, 2, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(723), null, "Buildable Star Wars-themed LEGO set.", "LEGO Star Wars Set", 7999m, 20001L, 5, 0m, new TimeSpan(183, 0, 0, 0, 0) },
                    { 7, 2, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(726), null, "Next-generation gaming console with ultra-high-speed SSD.", "PlayStation 5 Console", 49999m, 20002L, 0, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 8, 2, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(729), null, "Powerful gaming console with immersive gameplay.", "Xbox Series X", 49999m, 20003L, 0, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 9, 2, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(731), null, "Portable gaming console for versatile play.", "Nintendo Switch", 29999m, 20004L, 0, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 10, 2, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(734), null, "Classic board game for family and friends.", "Hasbro Monopoly Game", 1999m, 20005L, 0, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 11, 3, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(737), null, "Reliable sewing machine for all skill levels.", "Singer Sewing Machine", 15999m, 30001L, 20, 0m, new TimeSpan(1095, 0, 0, 0, 0) },
                    { 12, 3, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(743), null, "Versatile cutting machine for crafting projects.", "Cricut Maker Machine", 39999m, 30002L, 10, 0m, new TimeSpan(730, 0, 0, 0, 0) },
                    { 13, 3, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(746), null, "High-quality colored pencils for artists.", "Faber-Castell Colored Pencils", 2499m, 30003L, 5, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 14, 3, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(749), null, "Alcohol-based markers for smooth blending.", "Prismacolor Markers", 3999m, 30004L, 10, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 15, 3, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(766), null, "Premium watercolor paints for artists.", "Schmincke Watercolors", 5999m, 30005L, 5, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 16, 4, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(769), null, "Classic straight-fit jeans for men.", "Levi's Denim Jeans", 4999m, 40001L, 10, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 17, 4, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(772), null, "Comfortable and stylish sneakers for daily wear.", "Nike Air Max Sneakers", 8999m, 40002L, 15, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 18, 4, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(775), null, "Soft cotton T-shirt with modern fit.", "Calvin Klein T-shirt", 1999m, 40003L, 0, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 19, 4, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(778), null, "Iconic sunglasses with a timeless design.", "Ray-Ban Aviator Sunglasses", 14999m, 40004L, 10, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 20, 4, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(781), null, "Luxury leather handbag with modern style.", "Michael Kors Leather Handbag", 29999m, 40005L, 5, 0m, new TimeSpan(730, 0, 0, 0, 0) },
                    { 21, 5, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(783), null, "Powerful hair dryer with multiple heat settings.", "Revlon Hair Dryer", 3999m, 50001L, 10, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 22, 5, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(786), null, "Anti-aging cream for daily use.", "Olay Regenerist Cream", 2999m, 50002L, 5, 0m, new TimeSpan(365, 0, 0, 0, 0) },
                    { 23, 5, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(789), null, "Cordless electric shaver with precision blades.", "Philips Electric Shaver", 7999m, 50003L, 15, 0m, new TimeSpan(730, 0, 0, 0, 0) },
                    { 24, 5, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(792), null, "Rechargeable toothbrush with multiple brush heads.", "Oral-B Electric Toothbrush", 5999m, 50004L, 10, 0m, new TimeSpan(730, 0, 0, 0, 0) },
                    { 25, 5, new DateTime(2024, 9, 22, 17, 52, 44, 217, DateTimeKind.Local).AddTicks(795), null, "Moisturizing body wash for soft skin.", "Dove Body Wash", 1299m, 50005L, 0, 0m, new TimeSpan(365, 0, 0, 0, 0) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserId",
                table: "Addresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CartId",
                table: "AspNetUsers",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CartProducts_CartId",
                table: "CartProducts",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartProducts_ProductId",
                table: "CartProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_PromoCodeId",
                table: "Carts",
                column: "PromoCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_EditHistories_AddressId",
                table: "EditHistories",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_EditHistories_EditorId",
                table: "EditHistories",
                column: "EditorId");

            migrationBuilder.CreateIndex(
                name: "IX_EditHistories_ProductId",
                table: "EditHistories",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_EditHistories_PromoCodeId",
                table: "EditHistories",
                column: "PromoCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_EditHistories_ReviewId",
                table: "EditHistories",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_EditHistories_UserId",
                table: "EditHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProducts_OrderId",
                table: "OrderProducts",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProducts_ProductId",
                table: "OrderProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AddressId",
                table: "Orders",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PromoCodeId",
                table: "Orders",
                column: "PromoCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TransporterId",
                table: "Orders",
                column: "TransporterId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUser_WishlistUsersId",
                table: "ProductUser",
                column: "WishlistUsersId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnProductOrders_OrderId",
                table: "ReturnProductOrders",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnProductOrders_OrderProductId",
                table: "ReturnProductOrders",
                column: "OrderProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnProductOrders_TransporterId",
                table: "ReturnProductOrders",
                column: "TransporterId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProductId",
                table: "Reviews",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewerId",
                table: "Reviews",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_Searches_CategoryId",
                table: "Searches",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Searches_UserId",
                table: "Searches",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CartProducts");

            migrationBuilder.DropTable(
                name: "EditHistories");

            migrationBuilder.DropTable(
                name: "ProductUser");

            migrationBuilder.DropTable(
                name: "ReturnProductOrders");

            migrationBuilder.DropTable(
                name: "Searches");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "OrderProducts");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "PromoCodes");
        }
    }
}
