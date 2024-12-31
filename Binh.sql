
-- Tạo bảng Customers (Khách hàng)
CREATE TABLE Customers (
    customerId INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(100) NOT NULL,
    phone NVARCHAR(15),
    email NVARCHAR(100),
    address NVARCHAR(255),
    birthday DATE
);
GO

-- Tạo bảng Employees (Nhân viên)
CREATE TABLE Employees (
    employeeId INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(100) NOT NULL,
    phone NVARCHAR(15),
    email NVARCHAR(100),
    position NVARCHAR(50),
    salary DECIMAL(10, 2) NOT NULL
);
GO

-- Tạo bảng Cakes (Bánh)
CREATE TABLE Cakes (
    cakeId INT PRIMARY KEY IDENTITY(1,1),
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(255),
    price DECIMAL(10, 2) NOT NULL,
    category NVARCHAR(50),
    image NVARCHAR(255),
    stock INT NOT NULL DEFAULT 0
);
GO

-- Tạo bảng Orders (Đơn đặt hàng)
CREATE TABLE Orders (
    orderId INT PRIMARY KEY IDENTITY(1,1),
    customerId INT NOT NULL FOREIGN KEY REFERENCES Customers(customerId),
    employeeId INT FOREIGN KEY REFERENCES Employees(employeeId),
    orderDate DATETIME NOT NULL DEFAULT GETDATE(),
    deliveryDate DATETIME,
    status NVARCHAR(50) NOT NULL DEFAULT 'Processing',
    totalAmount DECIMAL(10, 2) NOT NULL
);
GO

-- Tạo bảng OrderDetails (Chi tiết đơn hàng)
CREATE TABLE OrderDetails (
    orderDetailId INT PRIMARY KEY IDENTITY(1,1),
    orderId INT NOT NULL FOREIGN KEY REFERENCES Orders(orderId),
    cakeId INT NOT NULL FOREIGN KEY REFERENCES Cakes(cakeId),
    quantity INT NOT NULL,
    price DECIMAL(10, 2) NOT NULL
);
GO

-- Tạo bảng Admins (Quản trị viên)
CREATE TABLE Admins (
    adminId INT PRIMARY KEY IDENTITY(1,1), -- Mã admin tự tăng
    username NVARCHAR(50) NOT NULL UNIQUE, -- Tên đăng nhập duy nhất
    password NVARCHAR(255) NOT NULL, -- Mật khẩu đã mã hóa
    name NVARCHAR(100) NOT NULL, -- Tên admin
    email NVARCHAR(100) NOT NULL, -- Email admin
    phone NVARCHAR(15), -- Số điện thoại
    role NVARCHAR(50) NOT NULL DEFAULT 'Admin', -- Vai trò (mặc định là Admin)
    createdAt DATETIME NOT NULL DEFAULT GETDATE(), -- Ngày tạo tài khoản
    updatedAt DATETIME NULL -- Ngày cập nhật tài khoản
);
GO
-- Tạo bảng Cart (Giỏ hàng)
CREATE TABLE Cart (
    cartId INT PRIMARY KEY IDENTITY(1,1), -- Mã giỏ hàng tự tăng
    customerId INT NOT NULL, -- Mã khách hàng (liên kết tới bảng Customers)
    cakeId INT NOT NULL, -- Mã bánh (liên kết tới bảng Cakes)
    quantity INT NOT NULL, -- Số lượng bánh
    price DECIMAL(10, 2) NOT NULL, -- Giá của bánh tại thời điểm thêm vào giỏ
    addedAt DATETIME NOT NULL DEFAULT GETDATE(), -- Thời gian thêm bánh vào giỏ
    FOREIGN KEY (customerId) REFERENCES Customers(customerId), -- Khóa ngoại tới bảng Customers
    FOREIGN KEY (cakeId) REFERENCES Cakes(cakeId) -- Khóa ngoại tới bảng Cakes
);
GO
ALTER TABLE Customers
ADD password NVARCHAR(255) NOT NULL;

-- Thêm dữ liệu vào bảng Customers
INSERT INTO Customers (name, phone, email, address, birthday,password)
VALUES 
    (N'Nguyễn Văn Bình', '0123456789', 'Binh@gmail.com', N'123 Lê Lợi, Hà Nội', '1990-01-15', N'1'),
    (N'Lê Thị B', '0987654321', 'lethib@example.com', N'456 Hoàng Hoa Thám, Đà Nẵng', '1995-07-20',N'1'),
    (N'Trần Văn C', '0912345678', 'tranvanc@example.com', N'789 Nguyễn Huệ, TP.HCM', '1985-12-05',N'1');

-- Thêm dữ liệu vào bảng Employees
INSERT INTO Employees (name, phone, email, position, salary)
VALUES 
    (N'Phạm Văn D', '0909876543', 'phamvand@example.com', N'Nhân viên bán hàng', 8000000),
    (N'Hoàng Thị E', '0934567890', 'hoangthie@example.com', N'Quản lý', 12000000);

-- Thêm dữ liệu vào bảng Cakes
INSERT INTO Cakes (name, description, price, category, image, stock)
VALUES 
    (N'Bánh kem dâu', N'Bánh kem tươi với vị dâu ngọt ngào', 200000, N'Bánh kem', '/images/cake1.jpg', 20),
    (N'Bánh chocolate', N'Bánh vị chocolate đậm đà', 250000, N'Bánh ngọt', '/images/cake2.jpg', 15),
    (N'Bánh trà xanh', N'Bánh vị trà xanh thơm mát', 220000, N'Bánh ngọt', '/images/cake3.jpg', 25);

-- Thêm dữ liệu vào bảng Orders
INSERT INTO Orders (customerId, employeeId, orderDate, deliveryDate, status, totalAmount)
VALUES 
    (1, 1, GETDATE(), DATEADD(DAY, 3, GETDATE()), N'Processing', 450000),
    (2, 2, GETDATE(), DATEADD(DAY, 5, GETDATE()), N'Shipping', 600000);

-- Thêm dữ liệu vào bảng OrderDetails
INSERT INTO OrderDetails (orderId, cakeId, quantity, price)
VALUES 
    (1, 1, 2, 200000),
    (1, 3, 1, 220000),
    (2, 2, 3, 250000);

-- Thêm dữ liệu vào bảng Admins
INSERT INTO Admins (username, password, name, email, phone, role)
VALUES 
    ('admin1', '1', N'Nguyễn Admin', 'admin@gmai.com', '0123456789', 'Admin'),
    ('admin2', '1', N'Lê Quản Trị', 'admin2@gmai.com', '0987654321', 'Manager');

-- Thêm dữ liệu vào bảng Cart
INSERT INTO Cart (customerId, cakeId, quantity, price)
VALUES 
    (1, 1, 1, 200000),
    (1, 2, 2, 250000),
    (2, 3, 1, 220000);
