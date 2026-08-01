// Fills a fresh local database with example employers, job postings, job seekers and applications
// through the real API — the same way `e2e/fixtures.ts` in HRMS-Frontend builds its test data.
// `DatabaseSeeder` only creates roles and the bootstrap admin, so a clean clone has nothing else:
// the public job board and company directory are empty on purpose, not broken.
//
// Usage (from this directory or the repo root):
//   node scripts/seed-demo-data.mjs
//
// Requires only Node 18+ (global fetch). No dependencies, no build step.
//
// Env vars, all optional:
//   API_BASE_URL     default http://localhost:5129
//   DEMO_PASSWORD    default "Parola123!" — used for every seeded account
//   EMPLOYER_COUNT   default 4
//   JOBSEEKER_COUNT  default 5
//
// Rate limiter: AuthController is rate-limited to 10 requests per 5 minutes per IP by default
// (RateLimiting:Auth:PermitLimit), and every register call goes through it. The defaults above
// total 9 register calls, one under that limit — job postings and applications are on other
// controllers and are not affected. Raising EMPLOYER_COUNT + JOBSEEKER_COUNT above 9, or re-running
// within the same 5-minute window, needs the limit raised for that run:
//
//   $env:RateLimiting__Auth__PermitLimit = "1000"
//   dotnet run --project Presentation/WebAPI --no-launch-profile

const API_BASE_URL = (process.env.API_BASE_URL ?? 'http://localhost:5129').replace(/\/+$/, '');
const DEMO_PASSWORD = process.env.DEMO_PASSWORD ?? 'Parola123!';
const EMPLOYER_COUNT = Number(process.env.EMPLOYER_COUNT ?? 4);
const JOBSEEKER_COUNT = Number(process.env.JOBSEEKER_COUNT ?? 5);

const RUN_ID = Date.now().toString(36);

async function api(path, { method = 'GET', token, body } = {}) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers: {
      'content-type': 'application/json',
      ...(token ? { authorization: `Bearer ${token}` } : {}),
    },
    body: body ? JSON.stringify(body) : undefined,
  });

  const text = await response.text();
  const envelope = text ? JSON.parse(text) : null;

  if (response.status === 429) {
    throw new Error(
      `${method} ${path} → 429 Too Many Requests. The auth endpoint's rate limit (10 per 5 min by ` +
        `default) is exhausted. Restart the API with a raised limit and re-run:\n\n` +
        `  $env:RateLimiting__Auth__PermitLimit = "1000"\n` +
        `  dotnet run --project Presentation/WebAPI --no-launch-profile\n`,
    );
  }

  if (!response.ok || envelope?.isSuccess === false) {
    throw new Error(`${method} ${path} → ${response.status}: ${envelope?.message ?? text}`);
  }

  return envelope.data;
}

function pick(list) {
  return list[Math.floor(Math.random() * list.length)];
}

function pickMany(list, count) {
  return [...list].sort(() => Math.random() - 0.5).slice(0, count);
}

function futureDateOnly(daysAhead) {
  const date = new Date();
  date.setDate(date.getDate() + daysAhead);
  return date.toISOString().slice(0, 10);
}

const EMPLOYERS = [
  {
    companyName: 'Akbank Teknoloji',
    sectors: ['Bankacılık', 'Finans'],
    numberOfEmployees: 850,
    webSite: 'https://akbank.com',
    description: 'Bankacılık sektörüne dijital ürünler geliştiren teknoloji ekibi.',
  },
  {
    companyName: 'Getir Teknoloji',
    sectors: ['E-ticaret', 'Lojistik'],
    numberOfEmployees: 1200,
    webSite: 'https://getir.com',
    description: 'Hızlı teslimat platformunun arkasındaki mühendislik organizasyonu.',
  },
  {
    companyName: 'Trendyol',
    sectors: ['E-ticaret', 'Perakende'],
    numberOfEmployees: 3500,
    webSite: 'https://trendyol.com',
    description: "Türkiye'nin en büyük e-ticaret platformlarından biri.",
  },
  {
    companyName: 'Turkcell Teknoloji',
    sectors: ['Telekomünikasyon'],
    numberOfEmployees: 2200,
    webSite: 'https://turkcell.com.tr',
    description: 'Telekomünikasyon altyapısı ve dijital servisler.',
  },
  {
    companyName: 'Insider',
    sectors: ['Pazarlama Teknolojileri', 'SaaS'],
    numberOfEmployees: 1400,
    webSite: 'https://useinsider.com',
    description: 'Küresel ölçekte büyüyen bir pazarlama teknolojisi şirketi.',
  },
  {
    companyName: 'Hepsiburada',
    sectors: ['E-ticaret'],
    numberOfEmployees: 2600,
    webSite: 'https://hepsiburada.com',
    description: 'Türkiye merkezli e-ticaret ve teknoloji şirketi.',
  },
];

const JOB_SEEKERS = [
  { firstName: 'Ayşe', lastName: 'Yılmaz' },
  { firstName: 'Mehmet', lastName: 'Demir' },
  { firstName: 'Zeynep', lastName: 'Kaya' },
  { firstName: 'Elif', lastName: 'Şahin' },
  { firstName: 'Can', lastName: 'Öztürk' },
  { firstName: 'Deniz', lastName: 'Aydın' },
  { firstName: 'Burak', lastName: 'Çelik' },
  { firstName: 'Selin', lastName: 'Arslan' },
];

const POSITIONS = [
  'Backend Developer',
  'Frontend Developer',
  'DevOps Engineer',
  'Data Analyst',
  'Product Manager',
  'QA Engineer',
  'UI/UX Designer',
  'Mobil Uygulama Geliştirici',
  'Site Reliability Engineer',
  'Sistem Yöneticisi',
];

const SKILL_POOL = [
  'C#', '.NET', 'PostgreSQL', 'React', 'TypeScript', 'Next.js', 'Docker', 'Kubernetes',
  'AWS', 'Python', 'SQL', 'Node.js', 'Java', 'Spring Boot', 'Redis', 'CI/CD', 'Terraform', 'Figma',
];

const CITIES = ['İstanbul', 'Ankara', 'İzmir', 'Bursa', 'Uzaktan'];
const EXPERIENCE_LEVELS = ['Stajyer', 'Junior', 'Orta seviye (2-4 yıl)', 'Kıdemli (5+ yıl)'];
const JOB_TYPES = ['FullTime', 'PartTime', 'Contract', 'Internship', 'Freelance'];

async function seedEmployer(seed, index) {
  const email = `${seed.companyName.toLowerCase().replace(/[^a-z]+/g, '-')}-${RUN_ID}${index}@example.com`;

  const auth = await api('/api/auth/register/employer', {
    method: 'POST',
    body: { email, password: DEMO_PASSWORD, ...seed },
  });

  console.log(`  + işveren: ${seed.companyName} (${email})`);

  return { ...seed, email, token: auth.accessToken, id: auth.user.id };
}

async function seedJobPosting(employer) {
  const title = pick(POSITIONS);
  const minSalary = 30000 + Math.floor(Math.random() * 8) * 10000;

  const posting = await api('/api/JobAdvertisements/add', {
    method: 'POST',
    token: employer.token,
    body: {
      title: `${title} - ${employer.companyName}`,
      jobPositionName: title,
      description:
        `${employer.companyName} bünyesinde ${title} pozisyonu için deneyimli ekip arkadaşları arıyoruz. ` +
        'Başvurular değerlendirilip kısa sürede dönüş yapılacaktır.',
      experience: pick(EXPERIENCE_LEVELS),
      city: pick(CITIES),
      skills: pickMany(SKILL_POOL, 2 + Math.floor(Math.random() * 3)),
      minSalary,
      maxSalary: minSalary + 20000 + Math.floor(Math.random() * 4) * 10000,
      currency: 'TRY',
      openPositions: 1 + Math.floor(Math.random() * 2),
      jobType: pick(JOB_TYPES),
      deadline: futureDateOnly(30 + Math.floor(Math.random() * 60)),
    },
  });

  return posting.id;
}

async function seedJobSeeker(seed, index) {
  const email =
    `${seed.firstName.toLowerCase()}.${seed.lastName.toLowerCase()}-${RUN_ID}${index}@example.com`
      .replace(/ı/g, 'i')
      .replace(/ş/g, 's')
      .replace(/ğ/g, 'g')
      .replace(/ü/g, 'u')
      .replace(/ö/g, 'o')
      .replace(/ç/g, 'c');

  const auth = await api('/api/auth/register/jobseeker', {
    method: 'POST',
    body: { email, password: DEMO_PASSWORD, firstName: seed.firstName, lastName: seed.lastName },
  });

  console.log(`  + iş arayan: ${seed.firstName} ${seed.lastName} (${email})`);

  return { ...seed, email, token: auth.accessToken };
}

async function seedApplication(seeker, jobAdvertisementId) {
  await api('/api/JobApplications/add', {
    method: 'POST',
    token: seeker.token,
    body: {
      jobAdvertisementId,
      jobSeekerNote: `Merhaba, bu pozisyona başvurmak istiyorum. — ${seeker.firstName} ${seeker.lastName}`,
    },
  });
}

async function main() {
  console.log(`API: ${API_BASE_URL}`);
  console.log(`Demo parola (tüm hesaplar için): ${DEMO_PASSWORD}\n`);

  console.log(`İşverenler oluşturuluyor (${EMPLOYER_COUNT})…`);
  const employers = [];
  for (const seed of EMPLOYERS.slice(0, EMPLOYER_COUNT)) {
    employers.push(await seedEmployer(seed, employers.length));
  }

  console.log('\nİlanlar yayınlanıyor…');
  const postingIds = [];
  for (const employer of employers) {
    const postingCount = 2 + Math.floor(Math.random() * 2);
    for (let i = 0; i < postingCount; i += 1) {
      postingIds.push(await seedJobPosting(employer));
    }
    console.log(`  + ${employer.companyName}: ${postingCount} ilan`);
  }

  console.log(`\nİş arayanlar oluşturuluyor (${JOBSEEKER_COUNT})…`);
  const seekers = [];
  for (const seed of JOB_SEEKERS.slice(0, JOBSEEKER_COUNT)) {
    seekers.push(await seedJobSeeker(seed, seekers.length));
  }

  console.log('\nBaşvurular gönderiliyor…');
  let applicationCount = 0;
  for (const seeker of seekers) {
    for (const jobId of pickMany(postingIds, Math.min(2, postingIds.length))) {
      await seedApplication(seeker, jobId);
      applicationCount += 1;
    }
  }
  console.log(`  + ${applicationCount} başvuru`);

  console.log('\nTamam. Örnek giriş bilgileri:');
  console.log(`  İşveren   : ${employers[0].email} / ${DEMO_PASSWORD}`);
  console.log(`  İş arayan : ${seekers[0].email} / ${DEMO_PASSWORD}`);
  console.log('\n/jobs ve /companies artık dolu olmalı.');
}

main().catch((error) => {
  console.error(`\nHata: ${error.message}`);
  process.exitCode = 1;
});
