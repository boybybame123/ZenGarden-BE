namespace ZenGarden.Domain.Enums;

public enum FocusActivityType
{
    // User-initiated activities
    Break,          // Nghỉ giải lao
    Distraction,    // Bị gián đoạn
    Note,           // Ghi chú

    // System activities
    System,         // Hoạt động hệ thống (pause, resume, complete)

    // Tab activities
    TabSwitch,      // Chuyển tab
    TabOpen,        // Mở tab mới
    TabClose,       // Đóng tab
    TabFocus,       // Focus vào tab

    // Window activities
    WindowOpen,     // Mở cửa sổ mới
    WindowClose,    // Đóng cửa sổ
    WindowMinimize, // Thu nhỏ cửa sổ
    WindowMaximize, // Phóng to cửa sổ
    WindowFocus,    // Focus vào cửa sổ

    // Browser activities
    BrowserOpen,    // Mở trình duyệt
    BrowserClose,   // Đóng trình duyệt

    // System state
    ScreenLock,     // Khóa màn hình
    ScreenUnlock,   // Mở khóa màn hình
    SystemSleep,    // Hệ thống ngủ
    SystemWake      // Hệ thống thức dậy
}