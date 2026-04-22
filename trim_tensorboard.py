import os
from tbparse import SummaryReader
from torch.utils.tensorboard import SummaryWriter

def trim_run(input_dir, output_dir, max_step):
    print(f"Leyendo: {input_dir}")
    reader = SummaryReader(input_dir)
    df = reader.scalars
    
    if df.empty:
        print("No se encontraron datos.")
        return

    print(f"Steps disponibles: {df['step'].min()} → {df['step'].max()}")
    print(f"Recortando hasta step: {max_step}")
    
    df_clean = df[df['step'] <= max_step]
    
    os.makedirs(output_dir, exist_ok=True)
    writer = SummaryWriter(log_dir=output_dir)
    
    for _, row in df_clean.iterrows():
        writer.add_scalar(row['tag'], row['value'], row['step'])
    
    writer.close()
    print(f"Guardado en: {output_dir}")
    print(f"Steps conservados: {df_clean['step'].max()}")

# ─── CONFIGURA ESTO ───────────────────────────────────────────
INPUT_DIR  = "results/maze_run_13/MazeAgent"   # tu run con los datos rotos
OUTPUT_DIR = "results/clean_run/MazeAgent"     # carpeta nueva con datos limpios
MAX_STEP   = 5305643                     # step donde empieza lo malo
# ──────────────────────────────────────────────────────────────

trim_run(INPUT_DIR, OUTPUT_DIR, MAX_STEP)