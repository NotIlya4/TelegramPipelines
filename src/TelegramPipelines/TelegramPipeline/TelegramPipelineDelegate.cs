﻿namespace TelegramPipelines.TelegramPipeline;

public delegate Task<TPipelineReturn?>TelegramPipelineDelegate<TPipelineReturn>(TelegramPipelineContext context);