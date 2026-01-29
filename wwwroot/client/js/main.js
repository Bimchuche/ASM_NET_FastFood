/* ================= LOAD DATA ================= */
let users = JSON.parse(localStorage.getItem("users")) || [];
let currentUser = JSON.parse(localStorage.getItem("currentUser")) || null;

let posts = JSON.parse(localStorage.getItem("posts"));
if (!Array.isArray(posts)) posts = [];

let editPostId = null;

/* ================= SAVE ================= */
const saveUsers = () =>
  localStorage.setItem("users", JSON.stringify(users));

const savePosts = () =>
  localStorage.setItem("posts", JSON.stringify(posts));

/* ================= HELPER ================= */
function getUserAvatar(name) {
  const u = users.find(x => x.name === name);
  return u?.avatar || "images/avatar.png";
}

/* ================= AUTH GUARD (🔥 FIX LOOP) ================= */
document.addEventListener("DOMContentLoaded", () => {
  const path = location.pathname;
  const user = JSON.parse(localStorage.getItem("currentUser"));

  // Trang PRIVATE
  if (!user && (path.includes("home") || path.includes("profile"))) {
    location.replace("login.html");
    return;
  }

  // Trang AUTH
  if (user && (path.includes("login") || path.includes("register"))) {
    location.replace("home.html");
    return;
  }

  // Load theo trang
  if (path.includes("home")) renderPosts();
  if (path.includes("index")) renderPostsPublic();
  if (path.includes("profile")) loadProfile();

  renderDashboardAvatar();
});

/* ================= REGISTER ================= */
function register() {
  const name = regName.value.trim();
  const email = regEmail.value.trim();
  const pass = regPassword.value.trim();

  if (!name || !email || !pass) {
    alert("Vui lòng nhập đầy đủ thông tin");
    return;
  }

  if (users.some(u => u.email === email)) {
    alert("Email đã tồn tại");
    return;
  }

  users.push({
    name,
    email,
    password: pass,
    avatar: "",
    birth: "",
    bio: "",
    createdAt: new Date().toLocaleDateString()
  });

  saveUsers();
  location.href = "login.html";
}

/* ================= LOGIN (🔥 KHÔNG RELOAD) ================= */
function login(e) {
  if (e) e.preventDefault();

  const email = loginEmail.value.trim();
  const pass = loginPassword.value.trim();

  const user = users.find(u => u.email === email && u.password === pass);
  if (!user) {
    alert("Sai email hoặc mật khẩu");
    return;
  }

  localStorage.setItem("currentUser", JSON.stringify(user));
  location.href = "home.html";
}

/* ================= LOGOUT ================= */
function logout() {
  localStorage.removeItem("currentUser");
  location.href = "index.html";
}

/* ================= PROFILE ================= */
function loadProfile() {
  if (!currentUser) return;

  profileAvatar.src = currentUser.avatar || "images/avatar.png";
  pname.value = currentUser.name;
  pemail.value = currentUser.email;
  pbirth.value = currentUser.birth || "";
  pbio.value = currentUser.bio || "";
  pcreated.value = currentUser.createdAt || "";
}

function updateProfile() {
  currentUser.name = pname.value.trim();
  currentUser.birth = pbirth.value;
  currentUser.bio = pbio.value;

  if (ppassword.value.trim())
    currentUser.password = ppassword.value.trim();

  const idx = users.findIndex(u => u.email === currentUser.email);
  users[idx] = currentUser;

  saveUsers();
  localStorage.setItem("currentUser", JSON.stringify(currentUser));

  renderDashboardAvatar();
  renderPosts();

  alert("Cập nhật thành công!");
}

/* ================= PUBLIC FEED ================= */
function renderPostsPublic() {
  const div = document.getElementById("posts");
  if (!div) return;

  div.innerHTML = "";
  posts.forEach(p => {
    div.innerHTML += `
      <div class="card mb-4 shadow-sm" style="max-width:700px;margin:auto">
        <img src="${p.image || 'images/post1.jpg'}"
             class="card-img-top"
             style="max-height:300px;object-fit:cover">
        <div class="card-body">
          <h5>${p.title || ""}</h5>
          <p>${p.content || ""}</p>
          <small>${p.author}</small>
          <button class="btn btn-sm btn-outline-secondary mt-2"
            onclick="needLogin()">Bình luận</button>
        </div>
      </div>`;
  });
}

function needLogin() {
  alert("Vui lòng đăng nhập");
  location.href = "login.html";
}

/* ================= ADD POST ================= */
function addPost() {
  if (!postTitle.value.trim() || !postContent.value.trim()) {
    alert("Nhập tiêu đề và nội dung");
    return;
  }

  const reader = new FileReader();
  reader.onload = () => {
    posts.unshift({
      id: Date.now(),
      title: postTitle.value.trim(),
      content: postContent.value.trim(),
      image: reader.result || "",
      author: currentUser.name,
      comments: []
    });

    savePosts();
    renderPosts();

    postTitle.value = "";
    postContent.value = "";
    postImage.value = "";

    bootstrap.Modal.getInstance(postModal).hide();
  };

  if (postImage.files[0]) reader.readAsDataURL(postImage.files[0]);
  else reader.onload();
}

/* ================= FEED ================= */
function renderPosts() {
  const div = document.getElementById("posts");
  if (!div) return;

  div.innerHTML = "";

  posts.forEach(p => {
    const isOwner = p.author === currentUser.name;

    div.innerHTML += `
      <div class="feed-card">

        <div class="feed-header">
          <div class="feed-user">
            <img src="${getUserAvatar(p.author)}" class="feed-avatar">
            <div>
              <div class="feed-user-name">${p.author}</div>
              <div class="feed-time">${new Date(p.id).toLocaleString()}</div>
            </div>
          </div>
        </div>

        <div class="feed-text">
          ${p.title ? `<div class="feed-title">${p.title}</div>` : ""}
          ${p.content ? `<div class="feed-content">${p.content}</div>` : ""}
        </div>

        ${p.image ? `
          <div class="feed-image-wrapper">
            <img src="${p.image}" class="feed-image"
                 onclick="openImage(this)"
                 onload="adjustImage(this)">
          </div>` : ""}

        <div class="comments">
          <div class="comment-input">
            <img src="${getUserAvatar(currentUser.name)}">
            <input placeholder="Viết bình luận..."
              onkeypress="comment(event, ${p.id})">
          </div>

          ${(p.comments || []).map(c => `
            <div class="comment-item">
              <img src="${getUserAvatar(c.name)}" class="comment-avatar">
              <div class="comment-bubble">
                <div class="comment-name">${c.name}</div>
                <div>${c.text}</div>
              </div>
            </div>`).join("")}
        </div>
      </div>`;
  });
}

/* ================= COMMENT ================= */
function comment(e, id) {
  if (e.key !== "Enter" || !e.target.value.trim()) return;

  const post = posts.find(p => p.id === id);
  post.comments.push({
    name: currentUser.name,
    text: e.target.value.trim(),
    time: new Date().toLocaleString()
  });

  savePosts();
  renderPosts();
}

/* ================= AVATAR ================= */
avatarInput?.addEventListener("change", e => {
  const file = e.target.files[0];
  if (!file) return;

  const reader = new FileReader();
  reader.onload = () => {
    currentUser.avatar = reader.result;
    localStorage.setItem("currentUser", JSON.stringify(currentUser));

    const idx = users.findIndex(u => u.email === currentUser.email);
    users[idx].avatar = reader.result;
    saveUsers();

    loadProfile();
    renderDashboardAvatar();
    renderPosts();
  };
  reader.readAsDataURL(file);
});

/* ================= DASHBOARD AVATAR ================= */
function renderDashboardAvatar() {
  const div = document.getElementById("dashboardAvatar");
  if (!div) return;

  if (currentUser) {
    div.innerHTML = currentUser.avatar
      ? `<img src="${currentUser.avatar}" style="width:100%;height:100%;border-radius:50%">`
      : currentUser.name[0].toUpperCase();

    div.onclick = () => location.href = "profile.html";
  } else {
    div.innerHTML = `<i class="bi bi-person-fill"></i>`;
    div.onclick = () => location.href = "login.html";
  }
}

/* ================= IMAGE ================= */
function adjustImage(img) {
  img.classList.add(
    img.naturalWidth > img.naturalHeight ? "landscape" : "portrait"
  );
}

function openImage(img) {
  if (document.querySelector(".image-overlay")) return;

  const o = document.createElement("div");
  o.className = "image-overlay";
  o.onclick = () => o.remove();

  const i = document.createElement("img");
  i.src = img.src;
  i.className = "overlay-img";
  i.onclick = e => e.stopPropagation();

  o.appendChild(i);
  document.body.appendChild(o);
}
