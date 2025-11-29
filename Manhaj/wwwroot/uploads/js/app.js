const defaultConfig = {
  platform_name: "منهج",
  platform_tagline: "منهجك نحو التفوق التعليمي",
  home_title: "طريقك إلى المعرفة يبدأ هنا",
  about_title: "لماذا تختار منهج؟",
  about_description: "منهج هي منصة تعليمية متكاملة توفر مسارًا منظمًا ومُحكمًا للطلاب نحو التفوق. نجمع بين معلمين مؤهلين ومحتوى تفاعلي عالي الجودة مع متابعة مستمرة لتقدمك",
  how_title: "كيف يعمل منهج؟",
  background_color: "#F5F5F5",
  surface_color: "#FFFFFF",
  text_color: "#0A2463",
  primary_color: "#0A2463",
  accent_color: "#FF7F11"
};

let config = { ...defaultConfig };
let allData = [];
let recordCount = 0;
let currentUser = null;
let selectedCourse = null;

const dataHandler = {
  onDataChanged(data) {
    allData = data;
    recordCount = data.length;
    updateHomeStats();
    
    if (currentUser) {
      if (currentUser.type === 'student') {
        renderStudentDashboard();
      } else if (currentUser.type === 'instructor') {
        renderInstructorDashboard();
      } else if (currentUser.type === 'admin') {
        renderAdminDashboard();
      }
    }
  }
};

function showToast(message, bgColor = null) {
  const toast = document.getElementById('toast');
  toast.textContent = message;
  if (bgColor) {
    toast.style.backgroundColor = bgColor;
  }
  toast.classList.add('show');
  setTimeout(() => toast.classList.remove('show'), 3000);
}

function showPage(pageId) {
  // For multi-page setup, redirect to the appropriate page
  window.location.href = pageId + '.html';
}

function scrollToSection(sectionId) {
  const section = document.getElementById(sectionId);
  if (section) section.scrollIntoView({ behavior: 'smooth' });
}

function showStudentSection(sectionId) {
  document.querySelectorAll('.dashboard-section').forEach(s => {
    s.classList.remove('active');
  });
  document.getElementById(`student-section-${sectionId}`).classList.add('active');
  
  document.querySelectorAll('.sidebar-item').forEach(i => i.classList.remove('active'));
  event.target.closest('.sidebar-item').classList.add('active');
}

function showInstructorSection(sectionId) {
  document.querySelectorAll('.dashboard-section').forEach(s => {
    s.classList.remove('active');
  });
  document.getElementById(`instructor-section-${sectionId}`).classList.add('active');
  
  document.querySelectorAll('.sidebar-item').forEach(i => i.classList.remove('active'));
  event.target.closest('.sidebar-item').classList.add('active');
}

function showAdminSection(sectionId) {
  document.querySelectorAll('.dashboard-section').forEach(s => {
    s.classList.remove('active');
  });
  document.getElementById(`admin-section-${sectionId}`).classList.add('active');
  
  document.querySelectorAll('.sidebar-item').forEach(i => i.classList.remove('active'));
  event.target.closest('.sidebar-item').classList.add('active');
}

function updateHomeStats() {
  const users = allData.filter(d => d.type === 'user');
  const courses = allData.filter(d => d.type === 'course');
  const teachers = users.filter(u => u.user_type === 'instructor');
  const students = users.filter(u => u.user_type === 'student');

  const statUsers = document.getElementById('home-stat-users');
  const statCourses = document.getElementById('home-stat-courses');
  const statTeachers = document.getElementById('home-stat-teachers');
  const statStudents = document.getElementById('home-stat-students');
  
  if (statUsers) statUsers.textContent = users.length;
  if (statCourses) statCourses.textContent = courses.length;
  if (statTeachers) statTeachers.textContent = teachers.length;
  if (statStudents) statStudents.textContent = students.length;
}

async function handleContactForm(e) {
  e.preventDefault();
  showToast('تم إرسال رسالتك بنجاح! سنتواصل معك قريباً', config.primary_color);
  document.getElementById('contact-form').reset();
}

async function handleLogin(e) {
  e.preventDefault();
  const email = document.getElementById('login-email').value;
  const userType = document.getElementById('login-user-type').value;

  currentUser = {
    email: email,
    name: email.split('@')[0],
    type: userType
  };

  // Store user in localStorage
  localStorage.setItem('currentUser', JSON.stringify(currentUser));

  if (userType === 'student') {
    window.location.href = 'student-dashboard.html';
  } else if (userType === 'instructor') {
    window.location.href = 'instructor-dashboard.html';
  } else if (userType === 'admin') {
    window.location.href = 'admin-dashboard.html';
  }

  showToast(`مرحباً بك ${currentUser.name}!`, config.primary_color);
}

async function handleRegister(e) {
  e.preventDefault();
  
  if (recordCount >= 999) {
    showToast('عذراً، تم الوصول للحد الأقصى من المستخدمين', '#ef4444');
    return;
  }

  const userData = {
    id: Date.now().toString(),
    type: 'user',
    user_type: document.getElementById('register-user-type').value,
    username: document.getElementById('register-username').value,
    email: document.getElementById('register-email').value,
    course_title: '',
    course_description: '',
    video_url: '',
    materials: '',
    quiz_data: '',
    status: 'active',
    created_at: new Date().toISOString()
  };

  const result = await window.dataSdk.create(userData);

  if (result.isOk) {
    showToast('تم إنشاء حسابك بنجاح!', config.primary_color);
    document.getElementById('register-form').reset();
    setTimeout(() => window.location.href = 'login.html', 1500);
  } else {
    showToast('حدث خطأ أثناء إنشاء الحساب', '#ef4444');
  }
}

function renderStudentDashboard() {
  const courses = allData.filter(d => d.type === 'course');
  const coursesCount = document.getElementById('student-courses-count');
  if (coursesCount) coursesCount.textContent = courses.length;

  const grid = document.getElementById('student-courses-grid');
  const myCourses = document.getElementById('student-my-courses');
  const explore = document.getElementById('student-explore-courses');

  if (courses.length === 0) {
    const emptyHTML = '<div class="empty-state" style="grid-column: 1/-1;"><div class="empty-icon">📚</div><p class="empty-text">لا توجد دورات متاحة حالياً</p></div>';
    if (grid) grid.innerHTML = emptyHTML;
    if (myCourses) myCourses.innerHTML = emptyHTML;
    if (explore) explore.innerHTML = emptyHTML;
    return;
  }

  if (grid) grid.innerHTML = '';
  if (myCourses) myCourses.innerHTML = '';
  if (explore) explore.innerHTML = '';

  courses.forEach(course => {
    const card = createCourseCard(course, true);
    if (grid) grid.appendChild(card.cloneNode(true));
    if (myCourses) myCourses.appendChild(card.cloneNode(true));
    if (explore) explore.appendChild(card.cloneNode(true));
  });
}

function createCourseCard(course, isStudent = false) {
  const card = document.createElement('div');
  card.className = 'course-card';
  card.innerHTML = `
    <div class="course-thumbnail">🎓</div>
    <div class="course-content">
      <h3 class="course-title">${course.course_title}</h3>
      <p class="course-description">${course.course_description}</p>
      ${isStudent ? `<button class="btn btn-primary btn-small" style="width: 100%;" onclick="viewCourse('${course.id}')">عرض الدورة</button>` : ''}
      ${!isStudent ? `
        <div class="course-footer">
          <div class="course-meta">
            <span class="badge ${course.video_url ? 'badge-success' : 'badge-warning'}">${course.video_url ? '✓ فيديو' : '✗ بدون فيديو'}</span>
            <span class="badge ${course.quiz_data ? 'badge-success' : 'badge-warning'}">${course.quiz_data ? '✓ اختبار' : '✗ بدون اختبار'}</span>
          </div>
        </div>
      ` : ''}
    </div>
  `;
  return card;
}

function viewCourse(courseId) {
  localStorage.setItem('selectedCourseId', courseId);
  window.location.href = 'course-view.html';
}

function selectQuizOption(btn) {
  btn.parentElement.querySelectorAll('.quiz-option').forEach(o => o.classList.remove('selected'));
  btn.classList.add('selected');
}

function submitQuiz() {
  const selected = document.querySelectorAll('.quiz-option.selected');
  const total = document.querySelectorAll('.quiz-question').length;
  
  if (selected.length < total) {
    showToast('الرجاء الإجابة على جميع الأسئلة', '#f59e0b');
    return;
  }

  showToast('تم إرسال إجاباتك بنجاح!', config.primary_color);
}

function backToStudentDashboard() {
  window.location.href = 'student-dashboard.html';
}

function renderInstructorDashboard() {
  const courses = allData.filter(d => d.type === 'course' && d.email === currentUser.email);
  const videos = courses.filter(c => c.video_url);
  const quizzes = courses.filter(c => c.quiz_data);

  const coursesCount = document.getElementById('instructor-courses-count');
  const videosCount = document.getElementById('instructor-videos-count');
  const quizzesCount = document.getElementById('instructor-quizzes-count');
  
  if (coursesCount) coursesCount.textContent = courses.length;
  if (videosCount) videosCount.textContent = videos.length;
  if (quizzesCount) quizzesCount.textContent = quizzes.length;

  const grid = document.getElementById('instructor-courses-grid');
  if (grid) {
    if (courses.length === 0) {
      grid.innerHTML = '<div class="empty-state" style="grid-column: 1/-1;"><div class="empty-icon">📚</div><p class="empty-text">لم تقم بإضافة أي دورات بعد</p></div>';
    } else {
      grid.innerHTML = '';
      courses.forEach(course => grid.appendChild(createCourseCard(course, false)));
    }
  }

  const activity = document.getElementById('instructor-activity');
  if (activity) {
    if (courses.length === 0) {
      activity.innerHTML = '<div class="empty-state"><div class="empty-icon">📋</div><p class="empty-text">لا توجد نشاطات</p></div>';
    } else {
      activity.innerHTML = '';
      courses.slice(-5).reverse().forEach(c => {
        const div = document.createElement('div');
        div.style.padding = '16px 0';
        div.style.borderBottom = '2px solid var(--background-gray)';
        div.innerHTML = `
          <div style="display: flex; justify-content: space-between; align-items: center;">
            <div>
              <strong style="color: var(--primary-blue);">${c.course_title}</strong>
              <p style="margin: 4px 0 0 0; font-size: 13px; opacity: 0.6;">تم الإضافة في ${new Date(c.created_at).toLocaleDateString('ar-SA')}</p>
            </div>
            <span style="font-size: 28px;">📚</span>
          </div>
        `;
        activity.appendChild(div);
      });
    }
  }

  const quizCourseSelect = document.getElementById('quiz-course');
  if (quizCourseSelect) {
    quizCourseSelect.innerHTML = '<option value="">اختر دورة...</option>';
    courses.forEach(c => {
      const option = document.createElement('option');
      option.value = c.id;
      option.textContent = c.course_title;
      quizCourseSelect.appendChild(option);
    });
  }
}

async function handleInstructorUpload(e) {
  e.preventDefault();
  
  if (recordCount >= 999) {
    showToast('عذراً، تم الوصول للحد الأقصى من الدورات', '#ef4444');
    return;
  }

  const btn = document.getElementById('upload-submit-btn');
  btn.disabled = true;
  btn.innerHTML = '<span class="loading"></span>';

  const courseData = {
    id: Date.now().toString(),
    type: 'course',
    user_type: 'instructor',
    username: currentUser.name,
    email: currentUser.email,
    course_title: document.getElementById('upload-title').value,
    course_description: document.getElementById('upload-description').value,
    video_url: document.getElementById('upload-video-url').value,
    materials: document.getElementById('upload-materials').value,
    quiz_data: '',
    status: 'active',
    created_at: new Date().toISOString()
  };

  const result = await window.dataSdk.create(courseData);

  btn.disabled = false;
  btn.textContent = 'نشر الدورة';

  if (result.isOk) {
    showToast('تم نشر الدورة بنجاح!', config.primary_color);
    document.getElementById('instructor-upload-form').reset();
  } else {
    showToast('حدث خطأ أثناء النشر', '#ef4444');
  }
}

async function handleInstructorQuiz(e) {
  e.preventDefault();
  
  const courseId = document.getElementById('quiz-course').value;
  const questions = [];
  
  document.querySelectorAll('#quiz-builder-container .quiz-question').forEach(qDiv => {
    const inputs = qDiv.querySelectorAll('input');
    if (inputs.length >= 5) {
      questions.push({
        question: inputs[0].value,
        options: [inputs[1].value, inputs[2].value, inputs[3].value, inputs[4].value]
      });
    }
  });

  const course = allData.find(c => c.id === courseId);
  if (!course) return;

  course.quiz_data = JSON.stringify(questions);

  const result = await window.dataSdk.update(course);

  if (result.isOk) {
    showToast('تم حفظ الاختبار بنجاح!', config.primary_color);
  } else {
    showToast('حدث خطأ أثناء الحفظ', '#ef4444');
  }
}

function addQuizQuestion() {
  const container = document.getElementById('quiz-builder-container');
  const num = container.children.length + 1;
  
  const qDiv = document.createElement('div');
  qDiv.className = 'quiz-question';
  qDiv.style.background = 'var(--surface-white)';
  qDiv.style.marginTop = '24px';
  qDiv.innerHTML = `
    <label class="form-label">السؤال ${num}</label>
    <input type="text" class="form-input quiz-question-input" placeholder="نص السؤال" required style="margin-bottom: 12px;">
    <input type="text" class="form-input" placeholder="الخيار أ" required style="margin-bottom: 8px;">
    <input type="text" class="form-input" placeholder="الخيار ب" required style="margin-bottom: 8px;">
    <input type="text" class="form-input" placeholder="الخيار ج" required style="margin-bottom: 8px;">
    <input type="text" class="form-input" placeholder="الخيار د" required>
  `;
  container.appendChild(qDiv);
}

function renderAdminDashboard() {
  const users = allData.filter(d => d.type === 'user');
  const courses = allData.filter(d => d.type === 'course');
  const instructors = users.filter(u => u.user_type === 'instructor');
  const students = users.filter(u => u.user_type === 'student');

  const usersCount = document.getElementById('admin-users-count');
  const coursesCount = document.getElementById('admin-courses-count');
  const instructorsCount = document.getElementById('admin-instructors-count');
  const studentsCount = document.getElementById('admin-students-count');
  
  if (usersCount) usersCount.textContent = users.length;
  if (coursesCount) coursesCount.textContent = courses.length;
  if (instructorsCount) instructorsCount.textContent = instructors.length;
  if (studentsCount) studentsCount.textContent = students.length;

  const usersTable = document.getElementById('admin-users-table');
  if (usersTable) {
    if (users.length === 0) {
      usersTable.innerHTML = '<tr><td colspan="6"><div class="empty-state"><div class="empty-icon">👥</div><p class="empty-text">لا يوجد مستخدمون</p></div></td></tr>';
    } else {
      usersTable.innerHTML = '';
      users.forEach(u => {
        const row = document.createElement('tr');
        row.innerHTML = `
          <td>${u.username}</td>
          <td>${u.email}</td>
          <td><span class="badge" style="background: ${config.primary_color}20; color: ${config.primary_color};">${u.user_type === 'student' ? 'طالب' : 'معلم'}</span></td>
          <td><span class="badge badge-success">${u.status === 'active' ? 'نشط' : 'غير نشط'}</span></td>
          <td>${new Date(u.created_at).toLocaleDateString('ar-SA')}</td>
          <td><button class="btn btn-outline btn-small" onclick="toggleUserStatus('${u.__backendId}')">تعطيل</button></td>
        `;
        usersTable.appendChild(row);
      });
    }
  }

  const coursesGrid = document.getElementById('admin-courses-grid');
  if (coursesGrid) {
    if (courses.length === 0) {
      coursesGrid.innerHTML = '<div class="empty-state" style="grid-column: 1/-1;"><div class="empty-icon">📚</div><p class="empty-text">لا توجد دورات</p></div>';
    } else {
      coursesGrid.innerHTML = '';
      courses.forEach(c => {
        const card = document.createElement('div');
        card.className = 'course-card';
        card.innerHTML = `
          <div class="course-thumbnail">🎓</div>
          <div class="course-content">
            <h3 class="course-title">${c.course_title}</h3>
            <p class="course-description">${c.course_description}</p>
            <div class="course-footer">
              <span style="font-size: 13px; opacity: 0.7;">بواسطة: ${c.username}</span>
              <button class="btn btn-outline btn-small" onclick="deleteCourse('${c.__backendId}')">حذف</button>
            </div>
          </div>
        `;
        coursesGrid.appendChild(card);
      });
    }
  }

  const activity = document.getElementById('admin-activity');
  if (activity) {
    const allActivity = [...users, ...courses].sort((a, b) => 
      new Date(b.created_at) - new Date(a.created_at)
    ).slice(0, 5);

    if (allActivity.length === 0) {
      activity.innerHTML = '<div class="empty-state"><div class="empty-icon">📋</div><p class="empty-text">لا توجد نشاطات</p></div>';
    } else {
      activity.innerHTML = '';
      allActivity.forEach(item => {
        const div = document.createElement('div');
        div.style.padding = '16px 0';
        div.style.borderBottom = '2px solid var(--background-gray)';
        
        const icon = item.type === 'user' ? '👤' : '📚';
        const label = item.type === 'user' ? `مستخدم جديد: ${item.username}` : `دورة جديدة: ${item.course_title}`;
        
        div.innerHTML = `
          <div style="display: flex; justify-content: space-between; align-items: center;">
            <div>
              <strong style="color: var(--primary-blue);">${label}</strong>
              <p style="margin: 4px 0 0 0; font-size: 13px; opacity: 0.6;">${new Date(item.created_at).toLocaleDateString('ar-SA')}</p>
            </div>
            <span style="font-size: 28px;">${icon}</span>
          </div>
        `;
        activity.appendChild(div);
      });
    }
  }
}

async function toggleUserStatus(backendId) {
  const user = allData.find(u => u.__backendId === backendId);
  if (!user) return;

  user.status = user.status === 'active' ? 'inactive' : 'active';
  
  const result = await window.dataSdk.update(user);
  if (result.isOk) {
    showToast('تم تحديث حالة المستخدم', config.primary_color);
  } else {
    showToast('حدث خطأ', '#ef4444');
  }
}

async function deleteCourse(backendId) {
  const course = allData.find(c => c.__backendId === backendId);
  if (!course) return;

  const result = await window.dataSdk.delete(course);
  if (result.isOk) {
    showToast('تم حذف الدورة', config.primary_color);
  } else {
    showToast('حدث خطأ', '#ef4444');
  }
}

function handleLogout() {
  currentUser = null;
  selectedCourse = null;
  localStorage.removeItem('currentUser');
  localStorage.removeItem('selectedCourseId');
  window.location.href = 'index.html';
  showToast('تم تسجيل الخروج بنجاح', config.accent_color);
}

async function onConfigChange(newConfig) {
  document.querySelectorAll('[id$="-logo-text"], #logo-text-ar').forEach(el => {
    el.textContent = newConfig.platform_name || defaultConfig.platform_name;
  });

  const heroTitle = document.getElementById('hero-title');
  const heroSubtitle = document.getElementById('hero-subtitle');
  const aboutTitle = document.getElementById('about-title');
  const aboutDescription = document.getElementById('about-description');
  const howTitle = document.getElementById('how-title');
  
  if (heroTitle) heroTitle.textContent = newConfig.home_title || defaultConfig.home_title;
  if (heroSubtitle) heroSubtitle.textContent = newConfig.platform_tagline || defaultConfig.platform_tagline;
  if (aboutTitle) aboutTitle.textContent = newConfig.about_title || defaultConfig.about_title;
  if (aboutDescription) aboutDescription.textContent = newConfig.about_description || defaultConfig.about_description;
  if (howTitle) howTitle.textContent = newConfig.how_title || defaultConfig.how_title;

  const bgColor = newConfig.background_color || defaultConfig.background_color;
  const surfaceColor = newConfig.surface_color || defaultConfig.surface_color;
  const textColor = newConfig.text_color || defaultConfig.text_color;
  const primaryColor = newConfig.primary_color || defaultConfig.primary_color;
  const accentColor = newConfig.accent_color || defaultConfig.accent_color;

  document.documentElement.style.setProperty('--background-gray', bgColor);
  document.documentElement.style.setProperty('--surface-white', surfaceColor);
  document.documentElement.style.setProperty('--text-dark', textColor);
  document.documentElement.style.setProperty('--primary-blue', primaryColor);
  document.documentElement.style.setProperty('--accent-orange', accentColor);

  document.body.style.backgroundColor = bgColor;
  document.body.style.color = textColor;

  if (currentUser) {
    if (currentUser.type === 'student') renderStudentDashboard();
    else if (currentUser.type === 'instructor') renderInstructorDashboard();
    else if (currentUser.type === 'admin') renderAdminDashboard();
  }
}

async function init() {
  // Load current user from localStorage
  const storedUser = localStorage.getItem('currentUser');
  if (storedUser) {
    currentUser = JSON.parse(storedUser);
    
    // Update user name displays
    const studentName = document.getElementById('student-name');
    const studentWelcome = document.getElementById('student-welcome');
    const instructorName = document.getElementById('instructor-name');
    const adminName = document.getElementById('admin-name');
    
    if (studentName) studentName.textContent = currentUser.name;
    if (studentWelcome) studentWelcome.textContent = currentUser.name;
    if (instructorName) instructorName.textContent = currentUser.name;
    if (adminName) adminName.textContent = currentUser.name;
  }
  
  // Load selected course for course view page
  const selectedCourseId = localStorage.getItem('selectedCourseId');
  if (selectedCourseId && window.location.pathname.includes('course-view')) {
    // Will be loaded after data is fetched
  }
  
  const result = await window.dataSdk.init(dataHandler);
  if (!result.isOk) {
    showToast('حدث خطأ في تهيئة المنصة', '#ef4444');
  }
  
  // Load course view data if on course view page
  if (selectedCourseId && window.location.pathname.includes('course-view')) {
    selectedCourse = allData.find(c => c.id === selectedCourseId);
    if (selectedCourse) {
      document.getElementById('course-view-title').textContent = selectedCourse.course_title;
      document.getElementById('course-view-description').textContent = selectedCourse.course_description;
      document.getElementById('course-video-url').textContent = `رابط: ${selectedCourse.video_url}`;

      const materials = document.getElementById('course-materials');
      materials.innerHTML = selectedCourse.materials ? 
        `<p style="margin: 0; line-height: 1.8;">${selectedCourse.materials}</p>` : 
        '<p style="margin: 0; opacity: 0.6;">لا توجد مواد متاحة</p>';

      const quiz = document.getElementById('course-quiz');
      if (selectedCourse.quiz_data) {
        const questions = JSON.parse(selectedCourse.quiz_data);
        quiz.innerHTML = '';
        questions.forEach((q, idx) => {
          const qDiv = document.createElement('div');
          qDiv.className = 'quiz-question';
          qDiv.innerHTML = `
            <p class="quiz-question-text">${idx + 1}. ${q.question}</p>
            <div class="quiz-options">
              ${q.options.map(opt => `<button class="quiz-option" onclick="selectQuizOption(this)">${opt}</button>`).join('')}
            </div>
          `;
          quiz.appendChild(qDiv);
        });
      } else {
        quiz.innerHTML = '<p style="opacity: 0.6;">لا يوجد اختبار لهذه الدورة</p>';
      }
    }
  }
}

if (window.elementSdk) {
  window.elementSdk.init({
    defaultConfig,
    onConfigChange,
    mapToCapabilities: (cfg) => ({
      recolorables: [
        {
          get: () => cfg.background_color || defaultConfig.background_color,
          set: (value) => {
            cfg.background_color = value;
            window.elementSdk.setConfig({ background_color: value });
          }
        },
        {
          get: () => cfg.surface_color || defaultConfig.surface_color,
          set: (value) => {
            cfg.surface_color = value;
            window.elementSdk.setConfig({ surface_color: value });
          }
        },
        {
          get: () => cfg.text_color || defaultConfig.text_color,
          set: (value) => {
            cfg.text_color = value;
            window.elementSdk.setConfig({ text_color: value });
          }
        },
        {
          get: () => cfg.primary_color || defaultConfig.primary_color,
          set: (value) => {
            cfg.primary_color = value;
            window.elementSdk.setConfig({ primary_color: value });
          }
        },
        {
          get: () => cfg.accent_color || defaultConfig.accent_color,
          set: (value) => {
            cfg.accent_color = value;
            window.elementSdk.setConfig({ accent_color: value });
          }
        }
      ],
      borderables: [],
      fontEditable: undefined,
      fontSizeable: undefined
    }),
    mapToEditPanelValues: (cfg) => new Map([
      ["platform_name", cfg.platform_name || defaultConfig.platform_name],
      ["platform_tagline", cfg.platform_tagline || defaultConfig.platform_tagline],
      ["home_title", cfg.home_title || defaultConfig.home_title],
      ["about_title", cfg.about_title || defaultConfig.about_title],
      ["how_title", cfg.how_title || defaultConfig.how_title]
    ])
  });
  config = window.elementSdk.config;
}

init();
