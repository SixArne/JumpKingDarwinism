<script setup lang="ts">
    defineProps<{
    src: string
    }>();
</script>

<template>
    <div class="sketchfab-embed-wrapper"> 
        <iframe 
            class="portfolio-iframe"
            title="The cursed mountainside" 
            frameborder="0" 
            allowfullscreen 
            mozallowfullscreen="true" 
            webkitallowfullscreen="true" 
            allow="autoplay; fullscreen; xr-spatial-tracking"
            xr-spatial-tracking
            execution-while-out-of-viewport
            execution-while-not-rendered
            web-share
            :src="src"> 
        </iframe> 
    </div>
</template>

<style scoped>
.portfolio-iframe {
    width: 100%;
    height: 400px;
}

@media (max-width: 450px) {
    .portfolio-iframe {
        width: 100%;
        height: 200px;
    }
}
</style>

