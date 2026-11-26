import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import {
  AlertCircle,
  ArrowLeft,
  CheckCircle2,
  Eye,
  EyeOff,
  LogOut,
  ShieldQuestion,
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

const MODE_HEADER: Record<AuthMode, { title: string; subtitle: string }> = {
  LOGIN: {
    title: 'Đăng nhập',
    subtitle: 'Chào mừng bạn trở lại.',
  },
  REGISTER: {
    title: 'Đăng ký',
    subtitle: 'Tạo tài khoản HCShop để bắt đầu mua sắm.',
  },
  FORGOT: {
    title: 'Quên mật khẩu',
    subtitle: 'Xác minh câu hỏi bảo mật để đặt lại mật khẩu.',
  },
};

const INPUT_CLASS =
  'w-full h-11 px-4 rounded-xl bg-[#f4f5f7] border border-zinc-200/70 text-[13px] text-zinc-900 placeholder:text-zinc-400 focus:outline-none focus:bg-white focus:border-zinc-400 transition-colors disabled:opacity-60';

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
  const [showRegPassword, setShowRegPassword] = useState(false);
  const [showRegConfirm, setShowRegConfirm] = useState(false);
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

  const header = MODE_HEADER[mode];

  return (
    <div className="py-2 pb-16">
      {/* Top-Left Back Link */}
      <div className="mb-4">
        {mode === 'LOGIN' ? (
          <Link
            to="/"
            className="inline-flex items-center gap-1.5 text-xs font-medium text-zinc-500 hover:text-zinc-900 transition-colors"
          >
            <ArrowLeft className="w-3.5 h-3.5" />
            <span>Quay lại</span>
          </Link>
        ) : (
          <button
            type="button"
            onClick={() => switchMode('LOGIN')}
            className="inline-flex items-center gap-1.5 text-xs font-medium text-zinc-500 hover:text-zinc-900 transition-colors"
          >
            <ArrowLeft className="w-3.5 h-3.5" />
            <span>Quay lại đăng nhập</span>
          </button>
        )}
      </div>

      {/* Centered Heading */}
      <div className="text-center mb-7">
        <h1 className="text-2xl sm:text-[26px] font-bold text-zinc-900 tracking-tight">
          {header.title}
        </h1>
        <p className="text-[13px] text-zinc-400 mt-1">{header.subtitle}</p>
      </div>

      {/* Centered Auth Card */}
      <div
        className={`mx-auto bg-white rounded-[28px] border border-zinc-100 shadow-[0_14px_45px_rgba(15,23,42,0.06)] p-7 sm:p-9 space-y-5 ${
          mode === 'REGISTER' ? 'max-w-[500px]' : 'max-w-[440px]'
        }`}
      >
        {/* Active Signed-In Banner (only shown if already authenticated) */}
        {currentUser && (
          <div className="flex items-center justify-between gap-3 px-4 py-3 rounded-2xl bg-[#f4f5f7] border border-zinc-200/70">
            <div className="min-w-0">
              <div className="flex items-center gap-2">
                <span className="text-xs font-semibold text-zinc-900 truncate">
                  {currentUser.fullName}
                </span>
                <span className="px-2 py-0.5 rounded-full text-[10px] font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200/60 shrink-0">
                  {ROLE_LABELS[currentUser.role]}
                </span>
              </div>
              <p className="text-[11px] text-zinc-500 truncate">{currentUser.email}</p>
            </div>
            <button
              type="button"
              onClick={() => {
                logoutUser();
                setFeedback({ ok: true, text: 'Đã đăng xuất khỏi phiên hiện tại.' });
              }}
              className="inline-flex items-center gap-1 px-3 py-1.5 rounded-full bg-white border border-zinc-200 hover:bg-zinc-100 text-[11px] font-medium text-zinc-700 transition-colors shrink-0"
            >
              <LogOut className="w-3 h-3" />
              <span>Đăng xuất</span>
            </button>
          </div>
        )}

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

        {/* MODE 1: LOGIN (UC-02) */}
        {mode === 'LOGIN' && (
          <form onSubmit={handleLoginSubmit} className="space-y-4">
            <div>
              <label className="block text-xs font-semibold text-zinc-700 mb-2">
                Tài khoản
              </label>
              <input
                type="text"
                required
                maxLength={254}
                value={loginId}
                onChange={(e) => setLoginId(e.target.value)}
                placeholder="Nhập email / số điện thoại..."
                className={INPUT_CLASS}
              />
            </div>

            <div>
              <div className="flex items-center justify-between mb-2">
                <label className="text-xs font-semibold text-zinc-700">Mật khẩu</label>
                <button
                  type="button"
                  onClick={() => {
                    setRecoveryIdentifier(loginId);
                    switchMode('FORGOT');
                  }}
                  className="text-xs font-medium text-blue-600 hover:underline"
                >
                  Quên mật khẩu?
                </button>
              </div>
              <div className="relative">
                <input
                  type={showLoginPassword ? 'text' : 'password'}
                  required
                  maxLength={128}
                  value={loginPassword}
                  onChange={(e) => setLoginPassword(e.target.value)}
                  placeholder="Nhập mật khẩu..."
                  className={`${INPUT_CLASS} pr-11`}
                />
                <button
                  type="button"
                  onClick={() => setShowLoginPassword((visible) => !visible)}
                  aria-label={showLoginPassword ? 'Ẩn mật khẩu' : 'Hiển thị mật khẩu'}
                  title={showLoginPassword ? 'Ẩn mật khẩu' : 'Hiển thị mật khẩu'}
                  className="absolute right-3 top-1/2 -translate-y-1/2 flex h-7 w-7 items-center justify-center rounded-full text-zinc-400 transition-colors hover:bg-zinc-200/70 hover:text-zinc-700"
                >
                  {showLoginPassword ? (
                    <EyeOff className="h-4 w-4" />
                  ) : (
                    <Eye className="h-4 w-4" />
                  )}
                </button>
              </div>
            </div>

            <div className="pt-2">
              <button
                type="submit"
                disabled={isSubmitting}
                className="w-full h-11 rounded-full bg-[#18181b] text-white text-[13px] font-semibold hover:bg-zinc-800 transition-colors disabled:opacity-60"
              >
                {isSubmitting ? 'Đang đăng nhập...' : 'Đăng nhập'}
              </button>
            </div>

            <p className="text-center text-xs text-zinc-600 pt-2">
              Chưa có tài khoản?{' '}
              <button
                type="button"
                onClick={() => switchMode('REGISTER')}
                className="font-semibold text-blue-600 hover:underline"
              >
                Đăng ký ngay
              </button>
            </p>
          </form>
        )}

        {/* MODE 2: REGISTER (UC-01) */}
        {mode === 'REGISTER' && (
          <form onSubmit={handleRegisterSubmit} className="space-y-4">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
              <div>
                <label className="block text-xs font-semibold text-zinc-700 mb-1.5">
                  Họ và tên *
                </label>
                <input
                  type="text"
                  required
                  maxLength={100}
                  value={regName}
                  onChange={(e) => setRegName(e.target.value)}
                  placeholder="Nguyễn Văn A"
                  className={INPUT_CLASS}
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-zinc-700 mb-1.5">
                  Số điện thoại *
                </label>
                <input
                  type="tel"
                  required
                  maxLength={15}
                  value={regPhone}
                  onChange={(e) => setRegPhone(e.target.value)}
                  placeholder="0988123456"
                  className={INPUT_CLASS}
                />
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold text-zinc-700 mb-1.5">
                Địa chỉ Email *
              </label>
              <input
                type="email"
                required
                maxLength={254}
                value={regEmail}
                onChange={(e) => setRegEmail(e.target.value)}
                placeholder="name@example.com"
                className={INPUT_CLASS}
              />
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
              <div>
                <label className="block text-xs font-semibold text-zinc-700 mb-1.5">
                  Mật khẩu *
                </label>
                <div className="relative">
                  <input
                    type={showRegPassword ? 'text' : 'password'}
                    required
                    maxLength={128}
                    value={regPassword}
                    onChange={(e) => setRegPassword(e.target.value)}
                    placeholder="Tối thiểu 8 ký tự..."
                    className={`${INPUT_CLASS} pr-10`}
                  />
                  <button
                    type="button"
                    onClick={() => setShowRegPassword((v) => !v)}
                    aria-label={showRegPassword ? 'Ẩn mật khẩu' : 'Hiển thị mật khẩu'}
                    className="absolute right-2.5 top-1/2 -translate-y-1/2 flex h-7 w-7 items-center justify-center rounded-full text-zinc-400 hover:bg-zinc-200/70 hover:text-zinc-700"
                  >
                    {showRegPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                  </button>
                </div>
              </div>

              <div>
                <label className="block text-xs font-semibold text-zinc-700 mb-1.5">
                  Xác nhận mật khẩu *
                </label>
                <div className="relative">
                  <input
                    type={showRegConfirm ? 'text' : 'password'}
                    required
                    maxLength={128}
                    value={regConfirm}
                    onChange={(e) => setRegConfirm(e.target.value)}
                    placeholder="Nhập lại mật khẩu..."
                    className={`${INPUT_CLASS} pr-10`}
                  />
                  <button
                    type="button"
                    onClick={() => setShowRegConfirm((v) => !v)}
                    aria-label={showRegConfirm ? 'Ẩn mật khẩu' : 'Hiển thị mật khẩu'}
                    className="absolute right-2.5 top-1/2 -translate-y-1/2 flex h-7 w-7 items-center justify-center rounded-full text-zinc-400 hover:bg-zinc-200/70 hover:text-zinc-700"
                  >
                    {showRegConfirm ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                  </button>
                </div>
              </div>
            </div>
            <p className="text-[11px] text-zinc-400 -mt-2">
              Mật khẩu tối thiểu 8 ký tự, bao gồm chữ hoa, chữ thường và chữ số.
            </p>

            {/* Required Security Question setup during registration (UC-01) */}
            <div className="p-4 rounded-2xl bg-[#f4f5f7] border border-zinc-200/70 space-y-2.5">
              <div className="flex items-center gap-2 text-xs font-semibold text-zinc-700">
                <ShieldQuestion className="w-4 h-4 text-blue-600 shrink-0" />
                <span>Câu hỏi bảo mật (bắt buộc) *</span>
              </div>
              <select
                value={regQuestion}
                onChange={(e) => setRegQuestion(e.target.value)}
                className="w-full h-10 px-3 rounded-xl bg-white border border-zinc-200/80 text-xs text-zinc-800 focus:outline-none focus:border-zinc-400"
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
                placeholder="Nhập câu trả lời bảo mật (tối thiểu 2 ký tự)..."
                className="w-full h-10 px-3 rounded-xl bg-white border border-zinc-200/80 text-xs text-zinc-800 placeholder:text-zinc-400 focus:outline-none focus:border-zinc-400"
              />
            </div>

            <div className="pt-1">
              <button
                type="submit"
                disabled={isSubmitting}
                className="w-full h-11 rounded-full bg-[#18181b] text-white text-[13px] font-semibold hover:bg-zinc-800 transition-colors disabled:opacity-60"
              >
                {isSubmitting ? 'Đang tạo tài khoản...' : 'Tạo tài khoản mới'}
              </button>
            </div>

            <p className="text-center text-xs text-zinc-600 pt-1">
              Đã có tài khoản?{' '}
              <button
                type="button"
                onClick={() => switchMode('LOGIN')}
                className="font-semibold text-blue-600 hover:underline"
              >
                Đăng nhập ngay
              </button>
            </p>
          </form>
        )}

        {/* MODE 3: FORGOT PASSWORD VIA SECURITY QUESTION (UC-03) */}
        {mode === 'FORGOT' && (
          <div className="space-y-4">
            {!recoveryUserId ? (
              <form onSubmit={handleLookupRecovery} className="space-y-4">
                <div>
                  <label className="block text-xs font-semibold text-zinc-700 mb-2">
                    Tài khoản cần khôi phục
                  </label>
                  <input
                    type="text"
                    required
                    maxLength={254}
                    value={recoveryIdentifier}
                    onChange={(e) => setRecoveryIdentifier(e.target.value)}
                    placeholder="Nhập email / số điện thoại đã đăng ký..."
                    className={INPUT_CLASS}
                  />
                </div>

                <div className="pt-1">
                  <button
                    type="submit"
                    disabled={isSubmitting}
                    className="w-full h-11 rounded-full bg-[#18181b] text-white text-[13px] font-semibold hover:bg-zinc-800 transition-colors disabled:opacity-60"
                  >
                    {isSubmitting ? 'Đang kiểm tra...' : 'Tiếp tục'}
                  </button>
                </div>

                <p className="text-center text-xs text-zinc-600 pt-1">
                  Nhớ lại mật khẩu?{' '}
                  <button
                    type="button"
                    onClick={() => switchMode('LOGIN')}
                    className="font-semibold text-blue-600 hover:underline"
                  >
                    Đăng nhập
                  </button>
                </p>
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
                  <div className="p-3 rounded-2xl bg-amber-50 border border-amber-200 text-xs text-amber-800">
                    Chức năng đặt lại mật khẩu tạm thời bị khóa 15 phút do nhập sai quá 5 lần. Vui lòng thử lại sau.
                  </div>
                )}

                <div>
                  <label className="block text-xs font-semibold text-zinc-700 mb-1.5">
                    Câu trả lời bảo mật *
                  </label>
                  <input
                    type="text"
                    required
                    maxLength={255}
                    disabled={recoveryLockedOut}
                    value={recoveryAnswer}
                    onChange={(e) => setRecoveryAnswer(e.target.value)}
                    placeholder="Nhập câu trả lời bảo mật..."
                    className={INPUT_CLASS}
                  />
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3.5">
                  <div>
                    <label className="block text-xs font-semibold text-zinc-700 mb-1.5">
                      Mật khẩu mới *
                    </label>
                    <input
                      type="password"
                      required
                      maxLength={128}
                      disabled={recoveryLockedOut}
                      value={recoveryNewPassword}
                      onChange={(e) => setRecoveryNewPassword(e.target.value)}
                      placeholder="Tối thiểu 8 ký tự..."
                      className={INPUT_CLASS}
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-zinc-700 mb-1.5">
                      Xác nhận mật khẩu *
                    </label>
                    <input
                      type="password"
                      required
                      maxLength={128}
                      disabled={recoveryLockedOut}
                      value={recoveryConfirmPassword}
                      onChange={(e) => setRecoveryConfirmPassword(e.target.value)}
                      placeholder="Nhập lại mật khẩu..."
                      className={INPUT_CLASS}
                    />
                  </div>
                </div>

                <div className="flex items-center gap-2.5 pt-2">
                  <button
                    type="button"
                    onClick={() => {
                      setRecoveryUserId(null);
                      setRecoveryLockedOut(false);
                    }}
                    className="px-4 h-11 rounded-full border border-zinc-200 text-xs font-medium text-zinc-700 hover:bg-zinc-50 transition-colors"
                  >
                    Đổi tài khoản
                  </button>
                  <button
                    type="submit"
                    disabled={recoveryLockedOut || isSubmitting}
                    className="flex-1 h-11 rounded-full bg-[#18181b] text-white text-[13px] font-semibold hover:bg-zinc-800 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
                  >
                    {isSubmitting ? 'Đang cập nhật...' : 'Đặt lại mật khẩu'}
                  </button>
                </div>
              </form>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
