#!/usr/bin/env python3
"""
02_seed.sql を生成するスクリプト。
実行: python infrastructure/db/seed/generate_seed.py > infrastructure/db/seed/02_seed.sql
DATE_TO は常に昨日。デモリセット時のバックフィル基準日と一致させる。
"""
import random
from datetime import datetime, timedelta

# --- 設定 ---
SEED         = 42
DATE_FROM    = datetime(2025, 12, 1)
DATE_TO      = datetime.now().replace(hour=0, minute=0, second=0, microsecond=0) - timedelta(days=1)
CORRECT_RATE = 0.05

random.seed(SEED)

employees = [
    ("EMP-001", 15),
    ("EMP-002", 30),
    ("EMP-003",  1),
]

def weekdays(date_from: datetime, date_to: datetime):
    d = date_from
    while d <= date_to:
        if d.weekday() < 5:
            yield d
        d += timedelta(days=1)

def random_clock_in(base_date: datetime) -> datetime:
    minutes = random.randint(0, 60)
    return base_date.replace(hour=8, minute=30) + timedelta(minutes=minutes)

def worked_minutes(round_unit: int) -> int:
    r = random.random()
    if r < 0.70:
        raw = 480 + random.randint(-10, 10)
    elif r < 0.90:
        raw = 480 + random.randint(60, 180)
    else:
        raw = random.randint(360, 420)
    return (raw // round_unit) * round_unit

print("-- 勤怠サンプルデータ")
print(f"-- 生成期間: {DATE_FROM.strftime('%Y-%m-%d')} 〜 {DATE_TO.strftime('%Y-%m-%d')}")
print()

total = 0
for emp_id, round_unit in employees:
    print(f"-- {emp_id}")
    for day in weekdays(DATE_FROM, DATE_TO):
        clock_in  = random_clock_in(day)
        minutes   = worked_minutes(round_unit)
        clock_out = clock_in + timedelta(minutes=minutes)
        corrected = "TRUE" if random.random() < CORRECT_RATE else "FALSE"
        print(
            f"INSERT INTO attendance_logs "
            f"(employee_id, clock_in, clock_out, is_corrected) VALUES ("
            f"'{emp_id}', "
            f"'{clock_in.strftime('%Y-%m-%d %H:%M:%S')}', "
            f"'{clock_out.strftime('%Y-%m-%d %H:%M:%S')}', "
            f"{corrected}"
            f") ON CONFLICT DO NOTHING;"
        )
        total += 1
    print()

print(f"-- 生成件数: {total} 件")
