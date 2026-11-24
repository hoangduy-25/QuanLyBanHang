import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import {
    AlertCircle,
    ArrowLeft,
    CheckCircle2,
    Eye,
    EyeOff,
    KeyRound,
    Lock,
    Mail,
    Phone,
    ShieldQuestion,
    User as UserIcon,
} from 'lucide-react';
import { useApp } from '../../context/AppContext';
import { ROLE_LABELS } from '../../utils/formatters';

type AuthMode = 'LOGIN' | 'REGISTER' | 'FORGOT';

const PRESET_QUESTIONS = [
  'Món ăn yêu thích?',
  'Trường đại học của bạn?',
  'Lớp đại học của bạn?',
  'Tên thú cưng đầu tiên của bạn?',
  'Thành phố nơi bạn sinh ra?',
];

export const AuthPage: React.FC = () => {
  const {
    currentUser,
    loginUser,
    registerUser,
    logoutUser,
    lookupUserForRecovery,
    resetPasswordBySecurityAnswer,
  } = useApp();
  const navigate = useNavigate();

  const [mode, setMode] = useState<AuthMode>('LOGIN');
  const [feedback, setFeedback] = useState<{ ok: boolean; text: string } | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Login state (UC-02)
  const [loginId, setLoginId] = useState('');
  const [loginPassword, setLoginPassword] = useState('');
  const [showLoginPassword, setShowLoginPassword] = useState(false);

  // Register state (UC-01)
  const [regName, setRegName] = useState('');
  const [regEmail, setRegEmail] = useState('');
  const [regPhone, setRegPhone] = useState('');
  const [regPassword, setRegPassword] = useState('');
  const [regConfirm, setRegConfirm] = useState('');
  const [regQuestion, setRegQuestion] = useState(PRESET_QUESTIONS[0]);
  const [regAnswer, setRegAnswer] = useState('');

  // Forgot Password state (UC-03)
  const [recoveryIdentifier, setRecoveryIdentifier] = useState('');
  const [recoveryUserId, setRecoveryUserId] = useState<string | null>(null);
  const [recoveryQuestion, setRecoveryQuestion] = useState<string>('');
  const [recoveryAnswer, setRecoveryAnswer] = useState('');
  const [recoveryNewPassword, setRecoveryNewPassword] = useState('');
  const [recoveryConfirmPassword, setRecoveryConfirmPassword] = useState('');
  const [recoveryLockedOut, setRecoveryLockedOut] = useState(false);

  const switchMode = (nextMode: AuthMode) => {
    setMode(nextMode);
    setFeedback(null);
    if (nextMode === 'FORGOT') {
      setRecoveryUserId(null);
      setRecoveryQuestion('');
      setRecoveryAnswer('');
      setRecoveryNewPassword('');
      setRecoveryConfirmPassword('');
      setRecoveryLockedOut(false);
    }
  };

  const handleLoginSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (isSubmitting) return;
    setIsSubmitting(true);
    try {
      const res = await loginUser(loginId, loginPassword);
      setFeedback({ ok: res.ok, text: res.message });
      if (res.ok && res.user) {
        setTimeout(() => {
          if (res.user?.role === 'QuanLy') {
            navigate('/portal');
          } else if (res.user?.role === 'NhanVien') {
            navigate('/portal/orders');
          } else {
            navigate('/');
          }
        }, 450);
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRegisterSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (isSubmitting) return;
    setIsSubmitting(true);
    try {
      const res = await registerUser({
        fullName: regName,
        email: regEmail,
        phone: regPhone,
        password: regPassword,
        confirmPassword: regConfirm,
        securityQuestion: regQuestion,
        securityAnswer: regAnswer,
      });
      setFeedback({ ok: res.ok, text: res.message });
      if (res.ok) {
        setTimeout(() => navigate('/'), 600);
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleLookupRecovery = async (e: React.FormEvent) => {
    e.preventDefault();
    if (isSubmitting) return;
    setIsSubmitting(true);
    try {
      const res = await lookupUserForRecovery(recoveryIdentifier);
      setFeedback({ ok: res.ok, text: res.message });
      setRecoveryLockedOut(Boolean(res.isLockedOut));
      if (res.ok && res.accountIdentifier && res.securityQuestion) {
        setRecoveryUserId(res.accountIdentifier);
        setRecoveryQuestion(res.securityQuestion);
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleResetPassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!recoveryUserId || isSubmitting) return;
    setIsSubmitting(true);
    try {
      const res = await resetPasswordBySecurityAnswer({
        accountIdentifier: recoveryUserId,
        answer: recoveryAnswer,
        newPassword: recoveryNewPassword,
        confirmPassword: recoveryConfirmPassword,
      });
      setFeedback({ ok: res.ok, text: res.message });
      if (res.isLockedOut) {
        setRecoveryLockedOut(true);
      }
      if (res.ok) {
        setRecoveryLockedOut(false);
        setTimeout(() => switchMode('LOGIN'), 800);
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="max-w-4xl mx-auto py-4 pb-16 space-y-6">
      <div>
        <Link
          to="/"
          className="inline-flex items-center gap-1.5 text-[13px] font-medium text-zinc-500 hover:text-zinc-900 transition-colors mb-2"
        >
          <ArrowLeft className="w-4 h-4" />
          Quay lại cửa hàng
        </Link>
        <h1 className="text-2xl font-bold text-zinc-900 tracking-tight">
          Tài khoản HCShop
        </h1>
        <p className="text-[13px] text-zinc-500 mt-0.5">
          Đăng nhập, tạo tài khoản thành viên hoặc khôi phục mật khẩu nhanh chóng
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        {/* Left column: Main Auth Card */}
        <div className="lg:col-span-7 bg-white rounded-3xl border border-zinc-200/80 p-6 sm:p-8 space-y-6">
          {/* Mode Tabs */}
          <div className="flex items-center gap-1.5 bg-[#f4f4f5] p-1 rounded-full">
            <button
              type="button"
              onClick={() => switchMode('LOGIN')}
              className={`flex-1 py-2 rounded-full text-[13px] font-semibold transition-all ${
                mode === 'LOGIN'
                  ? 'bg-white text-zinc-900 shadow-sm'
                  : 'text-zinc-500 hover:text-zinc-900'
              }`}
            >
              Đăng nhập
            </button>
            <button
              type="button"
              onClick={() => switchMode('REGISTER')}
              className={`flex-1 py-2 rounded-full text-[13px] font-semibold transition-all ${
                mode === 'REGISTER'
                  ? 'bg-white text-zinc-900 shadow-sm'
                  : 'text-zinc-500 hover:text-zinc-900'
              }`}
            >
              Đăng ký
            </button>
            <button
              type="button"
              onClick={() => switchMode('FORGOT')}
              className={`flex-1 py-2 rounded-full text-[13px] font-semibold transition-all ${
                mode === 'FORGOT'
                  ? 'bg-white text-zinc-900 shadow-sm'
                  : 'text-zinc-500 hover:text-zinc-900'
              }`}
            >
              Quên mật khẩu
            </button>
          </div>

          {/* Feedback Alert */}
          {feedback && (
            <div
              className={`p-3.5 rounded-2xl border text-[13px] flex items-start gap-2.5 ${
                feedback.ok
                  ? 'bg-emerald-50 border-emerald-200 text-emerald-800'
                  : 'bg-rose-50 border-rose-200 text-rose-800'
              }`}
            >
              {feedback.ok ? (
                <CheckCircle2 className="w-4 h-4 text-emerald-600 shrink-0 mt-0.5" />
              ) : (
                <AlertCircle className="w-4 h-4 text-rose-600 shrink-0 mt-0.5" />
              )}
              <span>{feedback.text}</span>
            </div>
          )}

          {/* TAB 1: LOGIN (UC-02) */}
          {mode === 'LOGIN' && (
            <form onSubmit={handleLoginSubmit} className="space-y-4">
              <div>
                <label className="block text-[12px] font-semibold text-zinc-700 mb-1.5">
                  Email hoặc Số điện thoại *
                </label>
                <div className="relative">
                  <Mail className="w-4 h-4 text-zinc-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
                  <input
                    type="text"
                    required
                    maxLength={254}
                    value={loginId}
                    onChange={(e) => setLoginId(e.target.value)}
                    placeholder="customer@hcshop.dev hoặc 0987654321"
                    className="w-full h-11 pl-10 pr-4 rounded-2xl bg-[#f8f9fa] border border-zinc-200/80 text-[13px] text-zinc-900 focus:outline-none focus:bg-white focus:border-zinc-900 transition-colors"
                  />
                </div>
              </div>

              <div>
                <div className="flex items-center justify-between mb-1.5">
                  <label className="text-[12px] font-semibold text-zinc-700">
                    Mật khẩu *
                  </label>
                  <button
                    type="button"
                    onClick={() => {
                      setRecoveryIdentifier(loginId);
                      switchMode('FORGOT');
                    }}
                    className="text-[12px] font-medium text-blue-600 hover:underline"
                  >
                    Quên mật khẩu?
                  </button>
                </div>
                    <div className="relative">
                        <Lock className="absolute left-3.5 top-1/2 h-4 w-4 -translate-y-1/2 text-zinc-400" />

                        <input
                            type={showLoginPassword ? 'text' : 'password'}
                            required
                            maxLength={128}
                            value={loginPassword}
                            onChange={(e) => setLoginPassword(e.target.value)}
                            placeholder="Nhập mật khẩu..."
                            className="h-11 w-full rounded-2xl border border-zinc-200/80 bg-[#f8f9fa] pl-10 pr-11 text-[13px] text-zinc-900 transition-colors focus:border-zinc-900 focus:bg-white focus:outline-none"
                        />

                        <button
                            type="button"
                            onClick={() => setShowLoginPassword((visible) => !visible)}
                            aria-label={showLoginPassword ? 'Ẩn mật khẩu' : 'Hiển thị mật khẩu'}
                            title={showLoginPassword ? 'Ẩn mật khẩu' : 'Hiển thị mật khẩu'}
                            className="absolute right-3.5 top-1/2 flex h-6 w-6 -translate-y-1/2 items-center justify-center rounded-full text-zinc-400 transition-colors hover:bg-zinc-200/70 hover:text-zinc-700"
                        >
                            {showLoginPassword ? (
                                <EyeOff className="h-4 w-4" />
                            ) : (
                                <Eye className="h-4 w-4" />
                            )}
                        </button>
                    </div>
              </div>

              <button
                type="submit"
                className="w-full h-11 rounded-full bg-zinc-900 text-white text-[13px] font-semibold hover:bg-zinc-800 transition-colors"
              >
                Đăng nhập vào HCShop
              </button>
            </form>
          )}

          {/* TAB 2: REGISTER (UC-01) */}
          {mode === 'REGISTER' && (
            <form onSubmit={handleRegisterSubmit} className="space-y-4">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
                <div>
                  <label className="block text-[12px] font-semibold text-zinc-700 mb-1">
                    Họ và tên *
                  </label>
                  <div className="relative">
                    <UserIcon className="w-4 h-4 text-zinc-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
                    <input
                      type="text"
                      maxLength={100}
                      value={regName}
                      onChange={(e) => setRegName(e.target.value)}
                      placeholder="Nguyễn Văn A"
                      className="w-full h-10 pl-10 pr-3.5 rounded-2xl bg-[#f8f9fa] border border-zinc-200/80 text-[13px] focus:outline-none focus:bg-white focus:border-zinc-900"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-[12px] font-semibold text-zinc-700 mb-1">
                                        Số điện thoại *
                                    </label>
                                    <div className="relative">
                                        <Phone className="w-4 h-4 text-zinc-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
                                        <input
                                            type="tel"
                                            maxLength={15}
                                            value={regPhone}
                                            onChange={(e) => setRegPhone(e.target.value)}
                                            placeholder="VD: 0988123456 hoặc +84988123456"
                      className="w-full h-10 pl-10 pr-3.5 rounded-2xl bg-[#f8f9fa] border border-zinc-200/80 text-[13px] focus:outline-none focus:bg-white focus:border-zinc-900"
                    />
                  </div>
                </div>
              </div>

              <div>
                <label className="block text-[12px] font-semibold text-zinc-700 mb-1">
                  Địa chỉ Email *
                </label>
                <div className="relative">
                  <Mail className="w-4 h-4 text-zinc-400 absolute left-3.5 top-1/2 -translate-y-1/2" />
                  <input
                    type="email"
                    maxLength={254}
                    value={regEmail}
                    onChange={(e) => setRegEmail(e.target.value)}
                    placeholder="name@example.com"
                    className="w-full h-10 pl-10 pr-3.5 rounded-2xl bg-[#f8f9fa] border border-zinc-200/80 text-[13px] focus:outline-none focus:bg-white focus:border-zinc-900"
                  />
                </div>
              </div>

            <div className="grid grid-cols-1 gap-3.5 sm:grid-cols-2">
                <div>
                    <label className="mb-1 flex min-h-8 items-start text-[12px] font-semibold leading-4 text-zinc-700">
                        Mật khẩu (tối thiểu 8 ký tự, có chữ hoa, thường, số) *
                    </label>
                    <input
                        type="password"
                        maxLength={128}
                        value={regPassword}
                        onChange={(e) => setRegPassword(e.target.value)}
                        placeholder="••••••"
                        className="h-10 w-full rounded-2xl border border-zinc-200/80 bg-[#f8f9fa] px-3.5 text-[13px] focus:border-zinc-900 focus:bg-white focus:outline-none"
                    />
                </div>

                <div>
                    <label className="mb-1 flex min-h-8 items-start text-[12px] font-semibold leading-4 text-zinc-700">
                        Xác nhận mật khẩu *
                    </label>
                    <input
                        type="password"
                        maxLength={128}
                        value={regConfirm}
                        onChange={(e) => setRegConfirm(e.target.value)}
                        placeholder="••••••"
                        className="h-10 w-full rounded-2xl border border-zinc-200/80 bg-[#f8f9fa] px-3.5 text-[13px] focus:border-zinc-900 focus:bg-white focus:outline-none"
                    />
                </div>
            </div>

              {/* Required Security Question setup during registration (UC-01) */}
              <div className="p-4 rounded-2xl bg-[#f8f9fa] border border-zinc-200/70 space-y-3">
                <div className="flex items-center gap-2 text-[12px] font-semibold text-zinc-800">
                  <ShieldQuestion className="w-4 h-4 text-blue-600" />
                  <span>Câu hỏi &amp; câu trả lời bảo mật (bắt buộc) *</span>
                </div>
                <select
                  value={regQuestion}
                  onChange={(e) => setRegQuestion(e.target.value)}
                  className="w-full h-9 px-3 rounded-xl bg-white border border-zinc-200 text-[12px] text-zinc-800 focus:outline-none"
                >
                  {PRESET_QUESTIONS.map((q) => (
                    <option key={q} value={q}>
                      {q}
                    </option>
                  ))}
                </select>
                <input
                  type="text"
                  required
                  minLength={2}
                  maxLength={255}
                  value={regAnswer}
                  onChange={(e) => setRegAnswer(e.target.value)}
                  placeholder="Nhập câu trả lời bảo mật (tối thiểu 2 ký tự) *"
                  className="w-full h-9 px-3 rounded-xl bg-white border border-zinc-200 text-[12px] text-zinc-800 focus:outline-none"
                />
              </div>

              <button
                type="submit"
                className="w-full h-11 rounded-full bg-zinc-900 text-white text-[13px] font-semibold hover:bg-zinc-800 transition-colors"
              >
                Tạo tài khoản mới
              </button>
            </form>
          )}

          {/* TAB 3: FORGOT PASSWORD VIA SECURITY QUESTION (UC-03, CRIT-02) */}
          {mode === 'FORGOT' && (
            <div className="space-y-4">
              {!recoveryUserId ? (
                <form onSubmit={handleLookupRecovery} className="space-y-4">
                  <p className="text-[13px] text-zinc-600 leading-relaxed">
                    Nhập Email hoặc Số điện thoại đã đăng ký để xác minh qua câu hỏi bảo mật.
                  </p>
                  <div>
                    <label className="block text-[12px] font-semibold text-zinc-700 mb-1.5">
                      Email hoặc Số điện thoại cần khôi phục *
                    </label>
                    <input
                      type="text"
                      required
                      maxLength={254}
                      value={recoveryIdentifier}
                      onChange={(e) => setRecoveryIdentifier(e.target.value)}
                      placeholder="VD: customer@hcshop.dev"
                      className="w-full h-11 px-4 rounded-2xl bg-[#f8f9fa] border border-zinc-200/80 text-[13px] focus:outline-none focus:bg-white focus:border-zinc-900"
                    />
                  </div>
                  <button
                    type="submit"
                    className="w-full h-11 rounded-full bg-zinc-900 text-white text-[13px] font-semibold hover:bg-zinc-800 transition-colors"
                  >
                    Tiếp tục kiểm tra Câu hỏi bảo mật
                  </button>
                </form>
              ) : (
                <form onSubmit={handleResetPassword} className="space-y-4">
                  <div className="p-3.5 rounded-2xl bg-blue-50/70 border border-blue-200/70 text-[13px] text-blue-950">
                    <div className="text-[11px] font-semibold uppercase tracking-wider text-blue-600 mb-0.5">
                      Câu hỏi bảo mật của bạn
                    </div>
                    <div className="font-bold">{recoveryQuestion}</div>
                  </div>

                  {recoveryLockedOut && (
                    <div className="p-3 rounded-2xl bg-amber-50 border border-amber-200 text-[12px] text-amber-800">
                      Chức năng đặt lại mật khẩu tạm thời bị khóa 15 phút do nhập sai quá 5 lần (UC-03 5a). Vui lòng thử lại sau.
                    </div>
                  )}

                  <div>
                    <label className="block text-[12px] font-semibold text-zinc-700 mb-1">
                      Câu trả lời bảo mật *
                    </label>
                    <input
                      type="text"
                      required
                      maxLength={255}
                      disabled={recoveryLockedOut}
                      value={recoveryAnswer}
                      onChange={(e) => setRecoveryAnswer(e.target.value)}
                      placeholder="Nhập câu trả lời (VD: Phở, HUCE, 68IT6)..."
                      className="w-full h-10 px-3.5 rounded-2xl bg-[#f8f9fa] border border-zinc-200/80 text-[13px] focus:outline-none focus:bg-white focus:border-zinc-900 disabled:opacity-60"
                    />
                  </div>

                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
                    <div>
                      <label className="block text-[12px] font-semibold text-zinc-700 mb-1">
                        Mật khẩu mới *
                      </label>
                      <input
                        type="password"
                        required
                        maxLength={128}
                        disabled={recoveryLockedOut}
                        value={recoveryNewPassword}
                        onChange={(e) => setRecoveryNewPassword(e.target.value)}
                        placeholder="Tối thiểu 6 ký tự"
                        className="w-full h-10 px-3.5 rounded-2xl bg-[#f8f9fa] border border-zinc-200/80 text-[13px] focus:outline-none focus:bg-white focus:border-zinc-900 disabled:opacity-60"
                      />
                    </div>
                    <div>
                      <label className="block text-[12px] font-semibold text-zinc-700 mb-1">
                        Xác nhận mật khẩu mới *
                      </label>
                      <input
                        type="password"
                        required
                        maxLength={128}
                        disabled={recoveryLockedOut}
                        value={recoveryConfirmPassword}
                        onChange={(e) => setRecoveryConfirmPassword(e.target.value)}
                        placeholder="Nhập lại mật khẩu mới"
                        className="w-full h-10 px-3.5 rounded-2xl bg-[#f8f9fa] border border-zinc-200/80 text-[13px] focus:outline-none focus:bg-white focus:border-zinc-900 disabled:opacity-60"
                      />
                    </div>
                  </div>

                  <div className="flex items-center gap-2.5 pt-1">
                    <button
                      type="button"
                      onClick={() => {
                        setRecoveryUserId(null);
                        setRecoveryLockedOut(false);
                      }}
                      className="px-4 h-11 rounded-full border border-zinc-200 text-[13px] font-medium text-zinc-700 hover:bg-zinc-50"
                    >
                      Đổi tài khoản
                    </button>
                    <button
                      type="submit"
                      disabled={recoveryLockedOut}
                      className="flex-1 h-11 rounded-full bg-zinc-900 text-white text-[13px] font-semibold hover:bg-zinc-800 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
                    >
                      Xác nhận đặt lại mật khẩu
                    </button>
                  </div>
                </form>
              )}
            </div>
          )}
        </div>

        {/* Right column: Demo Quick-Fill & Active Session Card */}
        <div className="lg:col-span-5 space-y-4">
          {/* Active Session status */}
          <div className="bg-white rounded-3xl border border-zinc-200/80 p-5 space-y-3">
            <div className="flex items-center justify-between">
              <span className="text-[11px] font-semibold uppercase tracking-wider text-zinc-400">
                Phiên đăng nhập hiện tại
              </span>
              {currentUser && (
                <span className="px-2.5 py-0.5 rounded-full text-[11px] font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200/60">
                  {ROLE_LABELS[currentUser.role]}
                </span>
              )}
            </div>

            {currentUser ? (
              <div className="flex items-center justify-between gap-3 pt-1">
                <div>
                  <div className="text-[14px] font-bold text-zinc-900">
                    {currentUser.fullName} ({currentUser.code})
                  </div>
                  <div className="text-[12px] text-zinc-500">
                    {currentUser.email} • {currentUser.phone}
                  </div>
                </div>
                <button
                  type="button"
                  onClick={() => {
                    logoutUser();
                    setFeedback({ ok: true, text: 'Đã đăng xuất khỏi phiên hiện tại.' });
                  }}
                  className="px-3.5 py-1.5 rounded-full bg-zinc-100 hover:bg-zinc-200 text-[12px] font-medium text-zinc-800 transition-colors"
                >
                  Đăng xuất
                </button>
              </div>
            ) : (
              <p className="text-[13px] text-zinc-500">
                Bạn đang ở chế độ <strong>Khách vãng lai (Guest)</strong>. Hãy đăng nhập bằng tài khoản SQL Server ở biểu mẫu bên trái.
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
